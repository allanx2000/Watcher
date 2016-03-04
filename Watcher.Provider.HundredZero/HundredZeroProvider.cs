using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Watcher.Extensions;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Provider.HundredZero
{
    public class HundredZeroProvider : AbstractProvider
    {
        public const string PROVIDER = "HundredZero";

        private const string BASE_URL = "http://hundredzeros.com/category/";

        private const string META_MAX_PAGES = "MaxPages";
        private const string META_URL = "CategoryUrl";

        public HundredZeroProvider() : base(PROVIDER, false, true)
        {
        }

        public override List<IMetaDataObject> GetMetaFields()
        {
            List<IMetaDataObject> meta = new List<IMetaDataObject>()
            {
                new MetaDataObject(META_URL, "Category URL"),
                new MetaDataObject(META_MAX_PAGES, "Max Pages")
            };

            return meta;
        }

        /*
        public override List<string> GetMetaFields()
        {
            return new List<string>() {
                META_URL,
                META_MAX_PAGES
            };
        }*/

        /*
        public override SourceOptions GetSourceOptions()
        {
            return SourceOptions.CreateFromParameters(true, false);
        }
        */

            
        protected override ISource DoCreateNewSource(string name, string url, List<IMetaDataObject> metaData)
        {
            bool valid = true;

            foreach (var md in metaData)
            {
                switch (md.ID)
                {
                    case META_URL:
                        if (!(md.Value is string && ((string)md.Value).StartsWith(BASE_URL)))
                            valid = false;
                        break;
                }

                if (!valid)
                    throw new Exception("Invalid URL");
            }

            AbstractSource s = new HundredZeroSource(name);

            s.SetMetaData(metaData);

            //TODO: Not needed?

            /*
            foreach (var kv in s.GetMetaData())
            {
                string displayName = null;
                switch(kv.Value.ID)
                {
                    case META_URL:
                        displayName = "Category URL";
                        break;
                    case META_MAX_PAGES:
                        displayName = "Max Pages";
                        break;
                }

                if (displayName != null)
                    kv.Value.SetDisplayName(displayName);
            }
            */

            return s;
        }

        protected override List<IDataItem> GetNewItems(ISource source)
        {
            if (source.ProviderID == PROVIDER)
            {

                int maxPages = Convert.ToInt32(source.GetMetaDataValue(META_MAX_PAGES));
                string url = source.GetMetaDataValue(META_URL).ToString();

                List<IDataItem> books = new List<IDataItem>();

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

        private List<IDataItem> GetBooksOnPage(ISource source, string baseUrl, int page)
        {
            List<IDataItem> books = new List<IDataItem>();

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

        public override void DoAction(IDataItem item)
        {
            Process.Start(item.ActionContent);
        }

    }
}
