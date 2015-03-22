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

namespace Watcher.Provider.Smack
{

    public class TEDProvider : AbstractProvider
    {
        public const string PROVIDER = "TEDProvider";

        private const string BASE_URL = "https://www.ted.com/talks";

        public TEDProvider()
            : base(PROVIDER)
        {

        }



        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            var source = new GenericSource("TED", PROVIDER);

            source.SetMetaData(metaData);

            return source;
        }

        private const int MaxPages = 1;

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            List<AbstractItem> items = new List<AbstractItem>();

            WebClient wc = new WebClient();
            for (int p = 1; p <= MaxPages; p++)
            {

                string page;
                string url = BASE_URL + "?page=" + p;

                page = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

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

        public override List<string> GetMetaFields()
        {
            return new List<string>();
        }
    }
}
