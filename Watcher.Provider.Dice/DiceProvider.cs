using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Watcher.Extensions;

namespace Watcher.Provider.Dice
{
    public class DiceProvider : AbstractProvider
    {
        public const string PROVIDER = "Dice";

        private const string BASE_URL = "https://www.dice.com/jobs/";

        private const string DefaultRange = "30"; //miles
        private const string DefaultPages = "5";
        private const string DefaultPageSize = "50";

        public const string META_QUERY = "Query";
        public const string META_LOCATION = "Location";
        public const string META_RANGE = "Range";

        public const string META_PAGES = "Pages";
        public const string META_PAGE_SIZE = "PageSize";
        
        public DiceProvider() : base(PROVIDER)
        {
        }

        public override List<string> GetMetaFields()
        {
            return new List<string>() {
                META_QUERY,
                META_LOCATION,
                META_RANGE,
                META_PAGES,
                META_PAGE_SIZE
            };
        }

        public override SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters(false, false);
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {

            if (!metaData.ContainsKey(META_QUERY) 
                || !metaData.ContainsKey(META_LOCATION) 
                || String.IsNullOrEmpty(metaData[META_QUERY])
                || String.IsNullOrEmpty(metaData[META_LOCATION])) 
                throw new Exception("Query and location is required");

            int tmp;

            if (!metaData.ContainsKey(META_PAGES))
                metaData.Add(META_PAGES, DefaultPages);
            else if (!int.TryParse(metaData[META_PAGES], out tmp))
                throw new Exception("Not a valid number");

            if (!metaData.ContainsKey(META_PAGE_SIZE))
                metaData.Add(META_PAGE_SIZE, DefaultRange);
            else if (!int.TryParse(metaData[META_PAGE_SIZE], out tmp))
                throw new Exception("Not a valid number");

            name = String.Format("{0}|{1}", metaData[META_QUERY], metaData[META_LOCATION]);

            AbstractSource s = new DiceSource(name);

            s.AddMetaData(META_QUERY, metaData[META_QUERY]);
            s.AddMetaData(META_LOCATION, metaData[META_LOCATION]);

            if (metaData.ContainsKey(META_RANGE) && String.IsNullOrEmpty(metaData[META_RANGE]))
            {
                int i;

                bool success = int.TryParse(metaData[META_RANGE], out i);

                if (success)
                    s.AddMetaData(META_RANGE, metaData[META_RANGE]);
                else
                    throw new Exception("Range is not a number");
            }
            else 
                s.AddMetaData(META_RANGE, DefaultRange);

            return s;
        }

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            if (source.ProviderID == PROVIDER)
            {
                int Pages;
                
                try
                {
                    Pages = int.Parse(source.GetMetaDataValue(META_PAGES));
                }
                catch
                {
                    Pages = int.Parse(DefaultPages);
                }

                int Size;

                try
                {
                    Size = int.Parse(source.GetMetaDataValue(META_PAGE_SIZE));
                }
                catch
                {
                    Size = int.Parse(DefaultPageSize);
                }


                string QUERY_BASE = CreateBaseUrl(source);

                List<AbstractItem> items = new List<AbstractItem>();

                try
                {
                    for (int i = 1; i <= Pages; i++)
                    {
                        string url = QUERY_BASE + "-startPage-" + i + "-limit-" + Size + "-jobs.html";
                        items.AddRange(GetItemsOnPage(url, source));
                    }
                }
                catch (Exception e)
                {
                    //Log
                }

                return items;
            }
            else return null;
        }

       
        private string CreateBaseUrl(AbstractSource source)
        {
            string query = source.GetMetaDataValue(META_QUERY);
            string location = source.GetMetaDataValue(META_LOCATION);
            string range = source.GetMetaDataValue(META_RANGE);

            string baseUrl = "https://www.dice.com/jobs/q-{0}-sort-date-l-{1}-radius-{2}";

            string url = String.Format(baseUrl, query,location, range);

            return url;
        }
                
        private List<BasicItem> GetItemsOnPage(string url, AbstractSource source)
        {
            List<BasicItem> items = new List<BasicItem>();

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
                title = linkNode.InnerText;
                link = linkNode.Attributes["href"].Value;

                if (link.Contains('?'))
                    link = link.Substring(0, link.IndexOf('?'));

                string id = GetId(link);

                location = i.SelectSingleNode(".//li[@class='location']").InnerText;
                posted = i.SelectSingleNode(".//li[@class='posted']").InnerText;

                string name = String.Format("{0} ({1}, {2})", title, location, id);

                var item = new BasicItem(source, name, link);

                items.Add(item);
             
            }

            return items;
        }

        private string GetId(string link)
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

        public override void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }



    }
}
