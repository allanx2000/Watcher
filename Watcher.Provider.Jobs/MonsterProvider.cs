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

namespace Watcher.Provider.Jobs
{
    public static class MonsterProvider
    {
        private const int DefaultPages = 5;

        public static AbstractSource DoCreateNewSource(string name, List<MetaDataObject> metaData) //URL is in meta
        {
            int tmp;

            //Set defaults
            MetaDataObject mdo = MetaDataObject.FindIn(metaData, JobsProvider.META_PAGES);

            bool hasUrl = false;
            bool hasPages = false;

            foreach (var m in metaData)
            {
                switch (m.ID)
                {
                    case JobsProvider.META_URL:
                        hasUrl = true;
                        break;
                    case JobsProvider.META_PAGES:
                        if (!int.TryParse(m.GetValueAsString(), out tmp))
                            throw new Exception("Not a valid number");
                        
                        mdo.SetValue(tmp);    
                        hasPages = true;
                        
                        break;
                }
            }

            if (!hasUrl)
                throw new Exception("Url must be provided.");

            if (!hasPages)
            {
                mdo = JobsProvider.MetaPages.Create();
                mdo.SetValue(DefaultPages);

                metaData.Add(mdo);
            }

            AbstractSource s = new JobsSource(name);
            s.SetMetaData(metaData);

            return s;
        
        }



        public static List<AbstractItem> GetNewItems(AbstractSource source)
        {
            if (source.GetMetaDataValue(JobsProvider.META_SOURCE).ToString() == Jobs.JobsProvider.SourceNames.Monster.ToString())
            {
                int Pages;

                try
                {
                    Pages = int.Parse(source.GetMetaDataValue(JobsProvider.META_PAGES).ToString());
                }
                catch
                {
                    Pages = int.Parse(DefaultPages.ToString());
                }

                ConcurrentBag<AbstractItem> items = new ConcurrentBag<AbstractItem>();
                
                int pageCounter = 0;
                object counterLock = new object();

                try
                {
                    for (int i = 1; i <= Pages; i++)
                    {
                        string url = source.GetMetaDataValue(JobsProvider.META_URL) + "&pg=" + i;

                        var pageItems = GetItemsOnPage(url, source);

                        FilterAndAdd(pageItems, items, pageCounter, counterLock);
                    }
                }
                catch (Exception e)
                {
                    //Log
                }

                while (true)
                {
                    lock(counterLock)
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

        private static List<JobItem> GetItemsOnPage(string url, AbstractSource source)
        {
            List<JobItem> items = new List<JobItem>();

            WebClient wc = new WebClient();

            string page = wc.DownloadString(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(page);

            var resultsNode = doc.DocumentNode.SelectSingleNode("//table[@class='listingsTable']");
            var odds = resultsNode.SelectNodes(".//tr[@class='odd']");
            var evens = resultsNode.SelectNodes(".//tr[@class='even']");

            items.AddRange(ParseItems(odds, source));
            items.AddRange(ParseItems(evens, source));

            return items;
        }

        private static List<JobItem> ParseItems(HtmlNodeCollection items, AbstractSource source)
        {
            List<JobItem> list = new List<JobItem>();

            foreach (var i in items)
            {
                string location, title, link;

                var linkNode = i.SelectSingleNode(".//div[@class='jobTitleContainer']/a");
                title = linkNode.InnerText;
                link = linkNode.Attributes["href"].Value;

                if (link.Contains('?'))
                    link = link.Substring(0, link.IndexOf('?'));

                string id = GetId(link);

                location = i.SelectSingleNode(".//div[@class='jobLocationSingleLine']/a").InnerText;

                string name = String.Format("{0} ({1}, {2})", title, location, id);

                var item = new JobItem(source, name, link);

                list.Add(item);
            }

            return list;
        }

        private static string GetId(string link)
        {
            return "H" + link.GetHashCode();
        }


        public static void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }




    }
}
