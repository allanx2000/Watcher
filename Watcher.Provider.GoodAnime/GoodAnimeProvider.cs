using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Watcher.Core;
using Watcher.Core.Items;

namespace Watcher.Provider.GoodAnime
{
    public class GoodAnimeProvider : AbstractProvider
    {
        //Used By GoodAnimeSource
        internal const string PROVIDER = "GoodAnimeProvider";

        internal const string BASE_URL = "http://www.goodanime.net/";


        private const string META_EXCLUSIONS = "Exclude";
        
        public GoodAnimeProvider()
            : base(PROVIDER)
        {

        }

        public override AbstractSource CastSource(AbstractSource src)
        {
            AbstractSource s = new GoodAnimeSource(src);

            return s;
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, Dictionary<string, string> metaData)
        {
            AbstractSource s = new GoodAnimeSource();

            return s;
        }

        private List<string> GetExclusions(AbstractSource source)
        {
            List<string> exclude = new List<string>();

            string exclusionString = source.GetMetaDataValue(META_EXCLUSIONS);
            if (!String.IsNullOrEmpty(exclusionString))
            {
                exclude.AddRange(from s in exclusionString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 select s.Trim());
            }

            return exclude;
        }

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            if (source.ProviderID == PROVIDER)
            {
                List<string> exclusions = GetExclusions(source);
                for (int i =0; i < exclusions.Count; i++)
                {
                    exclusions[i] = exclusions[i].ToLower();
                }

                List<AbstractItem> shows = new List<AbstractItem>();

                WebClient wc = new WebClient();
                string page = wc.DownloadString(BASE_URL);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);

                foreach (var table in doc.DocumentNode.SelectNodes("//table"))
                {
                    if (table.InnerText.Contains("(Sub)")) //Find Table
                    {
                        var items = table.SelectNodes(".//li");

                        foreach (var i in items)
                        {
                            if (i.InnerText.Contains("(Sub)")) //Get Subbed only
                            {
                                var link = i.SelectSingleNode(".//a");

                                string url = link.Attributes["href"].Value;
                                string name = link.InnerText;
                                name = WebUtility.HtmlDecode(name);
                                
                                if (!IsExcluded(name, exclusions))
                                {
                                    Show show = new Show(source, name, url);
                                    shows.Add(show);
                                }
                            }
                        }
                    }
                }

                return shows;
            }
            else return null;
        }

        private bool IsExcluded(string name, IEnumerable<string> exclusions)
        {
            name = name.ToLower();

            foreach (var e in exclusions)
            {
                
                if (name.Contains(e))
                    return true;
            }

            return false;
        }


        public override List<string> GetMetaFields()
        {
            
            return new List<string>() {
                META_EXCLUSIONS
            };
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
