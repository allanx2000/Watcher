using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Smack
{
    public class SmackProvider : AbstractProvider
    {
        public const string PROVIDER = "SmackProvider";
        
        public const string META_PAGES = "Pages";
        
        public const string META_SOURCE = "Source";

        private int DefaultPages = 3;

        public SmackProvider() : base(PROVIDER, false, false)
        {
        }

        public static class SourceNames
        {
            public const String ChinaSmack = "ChinaSmack";
            public const String JapanCrush = "JapanCrush";
        }

        //NOTE: Validation depends on selected source type

        public static readonly List<string> Sources = new List<string>()
        {
            SourceNames.ChinaSmack,
            SourceNames.JapanCrush,
        };

        public static readonly MetaDataObjectBuilder MetaSource = new MetaDataObjectBuilder(META_SOURCE, "Source", MetaDataObject.Type.Selector, Sources);
        public static readonly MetaDataObjectBuilder MetaPages = new MetaDataObjectBuilder(META_PAGES, "Pages");
        
        public override List<MetaDataObject> GetMetaFields()
        {
            List<MetaDataObject> TEMPLATE = new List<MetaDataObject>()
            {
                MetaSource.Create(),
                MetaPages.Create(),
            };

            return TEMPLATE;
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, List<MetaDataObject> templateAndValues)
        {
            string value;
            int tmp;

            MetaDataObject source = MetaDataObject.FindIn(templateAndValues, SmackProvider.META_SOURCE);
            value = source.GetValueAsString();

            if (String.IsNullOrEmpty(value))
                throw new Exception("No source selected");

            AbstractSource s = new SmackSource(value);

            MetaDataObject pages = MetaDataObject.FindIn(templateAndValues, SmackProvider.META_PAGES);
            value = pages.GetValueAsString();

            if (String.IsNullOrEmpty(value))
                tmp = DefaultPages;
            else
                int.TryParse(value, out tmp);

            pages.SetValue(tmp);
            
            s.SetMetaData(templateAndValues);

            return s;
            
        }

        public override AbstractSource CastSource(GenericSource src)
        {
            return SmackSource.CreateFrom(src);
        }


        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            SmackSource js = SmackSource.CreateFrom(source);
            return GetNewItems(js);
        }

        private const string ChinaBase = "http://www.chinasmack.com/page/";
        private const string JapanBase = "http://www.japancrush.com/page/";

        private const string ChinaSelect = "//div[@class='excerpt-post-title']/a";
        private const string JapanSelect = "//div[@class='excerpt-post-title']/a";


        private List<AbstractItem> GetNewItems(SmackSource source)
        {
            string BaseUrl;
            string Selector;

            if (source.Source == SourceNames.ChinaSmack.ToString())
            {
                BaseUrl = ChinaBase;
                Selector = ChinaSelect;
            }
            else if (source.Source == SourceNames.JapanCrush.ToString())
            {
                BaseUrl = JapanBase;
                Selector = JapanSelect;
            }
            else
                throw new Exception(source + "not accepted");

            List<AbstractItem> items = new List<AbstractItem>();

            WebClient wc = new WebClient();

            int max = Convert.ToInt32(source.Pages);
            for (int p = 1; p <= max; p++)
            {

                string url = BaseUrl + p;
                string page;
                
                page = wc.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                var itemNodes = doc.DocumentNode.SelectNodes(Selector);
                
                foreach (var a in itemNodes)
                {
                    try
                    {
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
    }
}
