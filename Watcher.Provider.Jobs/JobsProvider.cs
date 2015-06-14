using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Jobs
{
    public class JobsProvider : AbstractProvider
    {
        public const string PROVIDER = "JobsProvider";

        private const string DefaultRange = "5"; //miles
        private const string DefaultPages = "5";
        private const string DefaultPageSize = "50";

        public const string META_URL = "mURL";
        public const string META_QUERY = "Query";
        public const string META_LOCATION = "Location";
        public const string META_RANGE = "Range";

        public const string META_PAGES = "Pages";
        public const string META_PAGE_SIZE = "PageSize";

        public const string META_SOURCE = "Source";

        public JobsProvider() : base(PROVIDER, false, true)
        {
        }

        public static class SourceNames
        {
            public const String Dice = "Dice";
            public const String Monster = "Monster";
        }

        //NOTE: Validation depends on selected source type

        public static readonly List<string> Sources = new List<string>()
        {
            SourceNames.Dice,
            SourceNames.Monster,
        };

        public static readonly MetaDataObjectBuilder MetaSource = new MetaDataObjectBuilder(META_SOURCE, "Source", MetaDataObject.Type.Selector, Sources);
        public static readonly MetaDataObjectBuilder MetaUrl = new MetaDataObjectBuilder(META_URL, "URL (Monster)");
        public static readonly MetaDataObjectBuilder MetaQuery = new MetaDataObjectBuilder(META_QUERY, "Query (non-Monster)");
        public static readonly MetaDataObjectBuilder MetaLocation = new MetaDataObjectBuilder(META_LOCATION, "Location (non-Monster)");
        public static readonly MetaDataObjectBuilder MetaRange = new MetaDataObjectBuilder(META_RANGE, "Range (non-Monster)");
        public static readonly MetaDataObjectBuilder MetaPages = new MetaDataObjectBuilder(META_PAGES, "Pages");
        public static readonly MetaDataObjectBuilder MetaPageSize = new MetaDataObjectBuilder(META_PAGE_SIZE, "Page size  (non-Monster)");
        
        public override List<MetaDataObject> GetMetaFields()
        {
            List<MetaDataObject> TEMPLATE = new List<MetaDataObject>()
            {
                MetaSource.Create(),
                MetaUrl.Create(),
                MetaQuery.Create(),
                MetaLocation.Create(),
                MetaRange.Create(),
                MetaPages.Create(),
                MetaPageSize.Create()
            };

            return TEMPLATE;
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, List<MetaDataObject> metaData)
        {
            return SourceFactory.CreateSource(name, metaData);
        }

        public override AbstractSource CastSource(GenericSource src)
        {
            return JobsSource.CreateFrom(src);
        }


        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            JobsSource js = JobsSource.CreateFrom(source);

            switch (js.Source)
            {
                case SourceNames.Dice:
                    return JobsScraper.GetFromDice(source);
                case SourceNames.Monster:
                    return JobsScraper.GetFromMonster(source);
                default:
                    return new List<AbstractItem>(); //throw exception?
            }
        }

        public override void DoAction(AbstractItem item)
        {
            Process.Start(item.ActionContent);
        }
    }
}
