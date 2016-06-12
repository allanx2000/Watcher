using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Provider.Jobs
{
    class RSSSource : AbstractSource
    {
        public RSSSource(ISource src) : this(src.SourceName)
        {
            if (src.ID.HasValue)
                this.SetID(src.ID.Value);

            this.SetMetaData(src.GetMetaData());
        }

        public RSSSource(string name) : base(name, RSSProvider.Provider)
        {

        }
        
        public override string GetDisplayName()
        {
            return string.Format("{0} > {1}", ProviderID, SourceName);
        }
    }
}
