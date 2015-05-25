using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Watcher.Extensions;

namespace Watcher.Provider.Monster
{
    public class MonsterProvider : AbstractProvider
    {
        public const string PROVIDER = "Monster";

        private const string DefaultPages = "5";
        
        //public const string META_URL = "Url";
        public const string META_PAGES = "Pages";
        
        public MonsterProvider()
            : base(PROVIDER)
        {
        }

        public override List<string> GetMetaFields()
        {
            return new List<string>() {
                META_PAGES
            };
        }

        public override SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters(true, true);
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            int tmp;

            if (!metaData.ContainsKey(META_PAGES))
                metaData.Add(META_PAGES, DefaultPages);
            else if (!int.TryParse(metaData[META_PAGES], out tmp))
                throw new Exception("Not a valid number");

            AbstractSource s = new MonsterSource(name);;
            s.SetMetaData(metaData);
            s.SetUrl(url);

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

                List<AbstractItem> items = new List<AbstractItem>();

                try
                {
                    for (int i = 1; i <= Pages; i++)
                    {
                        string url = source.Url + "&pg=" + i;

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
        
        private List<BasicItem> GetItemsOnPage(string url, AbstractSource source)
        {
            List<BasicItem> items = new List<BasicItem>();

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

        private List<BasicItem> ParseItems(HtmlNodeCollection items, AbstractSource source)
        {
            List<BasicItem> list = new List<BasicItem>();

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

                var item = new BasicItem(source, name, link);

                list.Add(item);
            }

            return list;
        }

        private string GetId(string link)
        {
            return "H" + link.GetHashCode();
        }

        public override void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }



    }
}
