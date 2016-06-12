using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Provider.Jobs
{
    public class RSSProvider : AbstractProvider
    {
        public const string Provider = "RSSProvider";

        private const string META_URL = "FeedUrl"; //Cannot be URL, reserved for Watcher. TODO: Need to add an alert somewhere.
        
        public RSSProvider() : base(Provider, false, true)
        {
        }

        public override List<IMetaDataObject> GetMetaFields()
        {
            List<IMetaDataObject> meta = new List<IMetaDataObject>()
            {
                new MetaDataObject(META_URL, "Feed URL")
            };

            return meta;
        }
        
        protected override ISource DoCreateNewSource(string name, string oldNotUsed, List<IMetaDataObject> metaData)
        {
            var md = metaData.FirstOrDefault(x => x.ID == META_URL);

            if (md == null || md.Value == null)
                throw new Exception("Feed URL is not set.");

            string url = (string) md.Value;
            GetFeed(url); //Validation

            AbstractSource s = new RSSSource(name);
            s.SetMetaData(metaData);

            return s;
        }

        
        public override ISource CastSource(ISource src)
        {
            return new RSSSource(src);        
        }


        protected override List<IDataItem> GetNewItems(ISource source)
        {
            List<IDataItem> items = new List<IDataItem>();

            string feedUrl = (string) source.GetMetaDataValue(META_URL);
            var feed = GetFeed(feedUrl);
            
            foreach (var p in feed.Items)
            {
                string title = p.Title.Text;

                var firstLink = p.Links.FirstOrDefault();
                if (firstLink == null)
                    continue;

                //TODO: Case for multiple links? search for html type?

                string url = firstLink.Uri.AbsoluteUri;

                
                                
                BasicItem item = new BasicItem(source, title, url);
                items.Add(item);
            }

            return items;
        }

        private SyndicationFeed GetFeed(string url)
        {
            try
            {
                var req = HttpWebRequest.Create(url);
                var res = req.GetResponse().GetResponseStream();
                XmlReader responseReader = XmlReader.Create(res);
                SyndicationFeed feed = SyndicationFeed.Load(responseReader);

                return feed;
            }
            catch (Exception e)
            {
                throw new Exception("Could not load feed from: " + url, e);
            }
        }

        public override void DoAction(IDataItem item)
        {
            Process.Start(item.ActionContent);
        }
    }
}
