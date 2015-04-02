using System.Diagnostics;
using System.Runtime.Versioning;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.TED
{

    public class TEDProvider : AbstractProvider
    {
        public const string PROVIDER = "TEDProvider";

        private const string BASE_URL = "https://www.ted.com/talks";

        public TEDProvider()
            : base(PROVIDER)
        {

        }

        
        private const string META_MAX_PAGES = "MaxPages";
        
        public override List<string> GetMetaFields()
        {
            return new List<string>() {
                META_MAX_PAGES
            };
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            var source = new TEDSource("TED");

            int maxPages = int.Parse(metaData[META_MAX_PAGES]);

            source.AddMetaData(META_MAX_PAGES, metaData[META_MAX_PAGES]);

            return source;
        }

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            List<AbstractItem> items = new List<AbstractItem>();

            WebClient wc = new WebClient();
            wc.UseDefaultCredentials = true;

            int maxPages = int.Parse(source.GetMetaDataValue(META_MAX_PAGES));

            for (int p = 1; p <= maxPages; p++)
            {

                string page;
                string url = BASE_URL + "?page=" + p;

                page = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                var resultsContainer = doc.DocumentNode.SelectSingleNode("//div[@id='browse-results']");
                var talks = resultsContainer.SelectNodes(".//div[@class='talk-link']");

                foreach (var talk in talks)
                {
                    var innerContainer = talk.SelectSingleNode(".//div[@class='media__message']");

                    var link = innerContainer.SelectSingleNode(".//a");
                    if (link != null)
                    {
                        string name = link.InnerText.Trim();
                        string link_url = link.Attributes["href"].Value;
                        link_url = "https://www.ted.com/" + link_url;

                        BasicItem item = new BasicItem(source, name, link_url);
                        items.Add(item);
                    }
                }
            }
            return items;
        }

        public override void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }

        public override SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters(false, false);
        }

    }
}
