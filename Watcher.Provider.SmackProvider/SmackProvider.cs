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
using Watcher.Core;
using Watcher.Core.Items;

namespace Watcher.Provider.Smack
{
    public class Post : AbstractItem
    {
        public Post(AbstractSource source, string title, string link, int? id = null, bool isNew = true,
            DateTime? addedDate = null)
            : base(source, title, link, id, isNew, addedDate)
        {

        }
    }

    public class SmackProvider : AbstractProvider
    {
        public const string PROVIDER = "SmackProvider";

        private const string ChinaSite = "http://www.chinasmack.com/";
        private const string JapanSite = "http://www.japancrush.com/";

        private const string ChinaType = "ChinaSMACK";
        private const string JapanType = "JapanCRUSH";

        public SmackProvider()
            : base(PROVIDER)
        {

        }

        private const string Site = "Site";
        public override List<string> GetMetaFields()
        {
            return new List<string> { Site };
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            if (metaData.ContainsKey(Site))
            {
                if (metaData[Site].Equals(JapanType, StringComparison.OrdinalIgnoreCase))
                    metaData[Site] = JapanType;
                else if (metaData[Site].Equals(ChinaType, StringComparison.OrdinalIgnoreCase))
                    metaData[Site] = ChinaType;
                else
                    ThrowBadSiteException();
            }
            else
                ThrowBadSiteException();

            var source = new GenericSource(metaData[Site], PROVIDER);
            source.SetMetaData(metaData);

            return source;
        }

        private void ThrowBadSiteException()
        {
            throw new Exception("Site must be: " + String.Join(", ", ChinaType, JapanType));
        }

        private const int MaxPages = 1;

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            List<AbstractItem> items = new List<AbstractItem>();

            WebClient wc = new WebClient();
            for (int p = 1; p <= MaxPages; p++)
            {

                string url;
                string page;

                switch (source.GetMetaDataValue(Site))
                {
                    case JapanType:
                        url = JapanSite;
                        break;
                    case ChinaType:
                    default:
                        url = ChinaSite;
                        break;
                }

                url += "/page/" + p;

                page = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                var itemNodes = doc.DocumentNode.SelectNodes("//div[@id='archive']");
                itemNodes = itemNodes[0].SelectNodes(".//li");

                foreach (var node in itemNodes)
                {
                    try
                    {

                        var a = node.SelectSingleNode(".//div[@class='excerpt-post-title']/a");

                        string name = WebUtility.HtmlDecode(a.InnerText);
                        string link = a.Attributes["href"].Value;

                        var post = new Post(source, name, link);
                        items.Add(post);
                    }
                    catch (Exception e)
                    {
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
