using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Watcher.Extensions.V2;
using System.Collections.Concurrent;
using System.Threading;
using Watcher.Interop;

namespace Watcher.Provider.Jobs
{
    public class DiceProvider
    {
        private const string BASE_URL = "https://www.dice.com/jobs/";

        private const string DefaultRange = "10"; //miles
        private const string DefaultPages = "5";
        private const string DefaultPageSize = "50";

        private static List<string> GetFieldList()
        {
            return new List<string>() {
                JobsProvider.META_QUERY,
                JobsProvider.META_LOCATION,
                JobsProvider.META_PAGE_SIZE,
                JobsProvider.META_PAGES,
                JobsProvider.META_RANGE,
            };
        }

        public static AbstractSource DoCreateNewSource(string name, string url, List<IMetaDataObject> metaData)
        {

            Dictionary<string, bool> hasFields = new Dictionary<string, bool>();
            foreach (var s in GetFieldList())
            {
                hasFields.Add(s, false);
            }


            string query = null, location = null;

            foreach (var md in metaData)
            {
                //TODO: Are nulls included here?

                switch (md.ID)
                {
                    case JobsProvider.META_QUERY:
                        if (String.IsNullOrEmpty(md.GetValueAsString()))
                            throw new Exception("Query is required");
                        query = md.GetValueAsString();
                            break;
                    case JobsProvider.META_LOCATION:
                        if (String.IsNullOrEmpty(md.GetValueAsString()))
                            throw new Exception("Location is required");
                        
                        location = md.GetValueAsString();
                        break;
                    case JobsProvider.META_PAGES:
                    case JobsProvider.META_PAGE_SIZE:
                    case JobsProvider.META_RANGE:

                        string val = md.GetValueAsString();
                        
                        if (!String.IsNullOrEmpty(val))
                            IsInt(val);
                        else
                        {
                            switch (md.ID)
                            {
                                case JobsProvider.META_PAGES:
                                    md.SetValue(DefaultPages);
                                    break;
                                case JobsProvider.META_PAGE_SIZE:
                                    md.SetValue(DefaultPageSize);
                                    break;
                                case JobsProvider.META_RANGE:
                                    md.SetValue(DefaultRange);
                                    break;
                            }
                        }
                        break;
                }

                if (hasFields.ContainsKey(md.ID))
                    hasFields[md.ID] = true;
            }

            if (!hasFields[JobsProvider.META_QUERY]
                || !hasFields[JobsProvider.META_LOCATION])
                throw new Exception("Query and location are required");

            //Check this
            
            name = String.Format("{0} | {1}", query, location);

            
            AbstractSource source = new JobsSource(name);
            source.SetMetaData(metaData);

            return source;
        }

        private static void IsInt(string p)
        {
            try
            {
                int.Parse(p);
            }
            catch (Exception e)
            {
                throw new Exception("Not a number", e);
            }
        }

        public static List<IDataItem> GetNewItems(ISource source)
        {
            if (source.GetMetaDataValue(JobsProvider.META_SOURCE).ToString() == Jobs.JobsProvider.SourceNames.Dice.ToString())
            {
                int pages = int.Parse(source.GetMetaDataValue(JobsProvider.META_PAGES).ToString());
                int size = int.Parse(source.GetMetaDataValue(JobsProvider.META_PAGE_SIZE).ToString());
                string rangeString = source.GetMetaDataValue(JobsProvider.META_RANGE).ToString();
                string query = source.GetMetaDataValue(JobsProvider.META_QUERY).ToString();
                string location = source.GetMetaDataValue(JobsProvider.META_LOCATION).ToString();

                string QUERY_BASE = CreateBaseUrl(query, location, rangeString);

                ConcurrentBag<IDataItem> items = new ConcurrentBag<IDataItem>();

                int pageCounter = 0;
                object counterLock = new object();

                try
                {
                    for (int i = 1; i <= pages; i++)
                    {
                        string url = QUERY_BASE + "-startPage-" + i + "-limit-" + size + "-jobs.html";

                        var pageItems = GetItemsOnPage(url, source);
                        
                        FilterUtil.FilterAndAdd(pageItems, items, pageCounter, counterLock);
                    }
                }
                catch (Exception e)
                {
                    //Log
                }

                while (true)
                {
                    lock (counterLock)
                    {
                        if (pageCounter == 0)
                            break;
                    }

                    Thread.Sleep(2000);
                }

                return items.ToList();
            }
            else return null;
        }


        private static string CreateBaseUrl(string query, string location, string range)
        {
            string baseUrl = "https://www.dice.com/jobs/q-{0}-sort-date-l-{1}-radius-{2}";

            string url = String.Format(baseUrl, query, location, range);

            return url;
        }


        private static List<JobItem> GetItemsOnPage(string url, ISource source)
        {
            List<JobItem> items = new List<JobItem>();

            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;

            string page = wc.DownloadString(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(page);

            var resultsNode = doc.DocumentNode.SelectSingleNode("//div[@id='serp']");
            var itemNodes = resultsNode.SelectNodes("div");

            foreach (var i in itemNodes)
            {
                string location, posted, title, link;

                var linkNode = i.SelectSingleNode(".//a[@class='dice-btn-link']");
                title = linkNode.InnerText.Trim();
                link = linkNode.Attributes["href"].Value;

                if (link.Contains('?'))
                    link = link.Substring(0, link.IndexOf('?'));

                string id = GetId(link);

                location = i.SelectSingleNode(".//li[@class='location']").InnerText.Trim();
                posted = i.SelectSingleNode(".//li[@class='posted']").InnerText.Trim();

                string name = String.Format("{0} ({1}, {2})", title, location, id);

                var item = new JobItem(source, name, link);

                items.Add(item);

            }

            return items;
        }

        private static string GetId(string link)
        {

            string[] parts = link.Split('/');

            int len = parts.Length;

            if (len > 7)
            {
                return parts[6];
            }
            else
            {
            }
            return "H" + link.GetHashCode();
        }
    }
}
