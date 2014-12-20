using Watcher.Core;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Watcher.Extensions;

namespace Watcher.Provider.HundredZero
{
    public class HundredZeroProvider : AbstractProvider
    {
        public const string PROVIDER = "HundredZero";

        private const string BASE_URL = "http://hundredzeros.com/category/";

        private const string META_MAX_PAGES = "MaxPages";
        private const string META_URL = "CategoryUrl";

        public HundredZeroProvider() : base(PROVIDER)
        {
        }

        public override List<string> GetMetaFields()
        {
            return new List<string>() {
                META_URL,
                META_MAX_PAGES
            };
        }

        public override SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters(true, false);
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            
            if (!metaData.ContainsKey(META_URL) || !metaData[META_URL].StartsWith(BASE_URL))
                throw new Exception("Invalid URL");

            AbstractSource s = new GenericSource(name, PROVIDER);

            s.AddMetaData(META_URL, metaData[META_URL]);
            s.AddMetaData(META_MAX_PAGES, metaData[META_MAX_PAGES]);

            return s;
        }

        /*public override AbstractSource CastSource(AbstractSource src)
        {
            AbstractSource s = new  GoodAnimeSource(src);

            return s;
        }*/

        /*protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            AbstractSource s = new GoodAnimeSource();

            return s;
        }*/

        /*private List<string> GetExclusions(AbstractSource source)
        {
            List<string> exclude = new List<string>();

            string exclusionString = source.GetMetaDataValue(META_EXCLUSIONS);
            if (!String.IsNullOrEmpty(exclusionString))
            {
                exclude.AddRange(from s in exclusionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 select s.Trim());
            }

            return exclude;
        }*/

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            if (source.ProviderID == PROVIDER)
            {
                
                int maxPages = Convert.ToInt32(source.GetMetaDataValue(META_MAX_PAGES));
                string url = source.GetMetaDataValue(META_URL);

                List<AbstractItem> books = new List<AbstractItem>();

                try
                {
                    for (int i = 1; i <= maxPages; i++)
                    {
                        books.AddRange(GetBooksOnPage(source, url, i));
                    }
                }
                catch (Exception e)
                {
                    //Log
                }

                return books;
            }
            else return null;
        }



        private List<Book> GetBooksOnPage(AbstractSource source, string baseUrl, int page)
        {
            List<Book> books = new List<Book>();

            string url = baseUrl + "/page/" + page;

            WebClient wc = new WebClient();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(wc.DownloadString(url));

            var bookNodes = doc.DocumentNode.SelectNodes("//div[@class='post-content']");

            foreach (var i in bookNodes)
            {
                var el = i.SelectSingleNode("h1");
                string title = el.InnerText;
                title = WebUtility.HtmlDecode(title);

                string link = el.SelectSingleNode("a").Attributes["href"].Value;

                Book book = new Book(source, title, link);
                books.Add(book);
            }

            return books;
        }

        public override void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }

    }
}
