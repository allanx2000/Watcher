using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Smack
{
    class SmackSource : AbstractSource
    {
        
        public SmackSource(string name) : base(name, SmackProvider.PROVIDER)
        {
        }
        
        public string Source
        {
            get
            {
                return MetaDataObject.FindIn(GetMetaData(), SmackProvider.META_SOURCE).GetValueAsString();
            }
            set
            {
                SetMetaDataValue(SmackProvider.META_SOURCE, value);
            }
        }

        public string Pages {
            get
            {
                return MetaDataObject.FindIn(GetMetaData(), SmackProvider.META_PAGES).GetValueAsString();
            }
            set
            {
                SetMetaDataValue(SmackProvider.META_PAGES, value);
            }
        }

        public override string GetDisplayName()
        {
            return String.Format("{0} > {1}", SmackProvider.PROVIDER, Source);
        }
        
        internal static SmackSource CreateFrom(AbstractSource source)
        {
            if (source is SmackSource)
                return (SmackSource) source;

            SmackSource js = new SmackSource(source.SourceName);
            js.SetMetaData(source.GetMetaData());
            js.SetID(source.ID.Value);
            
            return js;
        }
    }
}
