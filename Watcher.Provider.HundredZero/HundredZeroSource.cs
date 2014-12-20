using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Core;
using Watcher.Extensions;

namespace Watcher.Provider.HundredZero
{
    class HundredZeroSource : AbstractSource
    {        
        public HundredZeroSource() 
            : base("HundredZero", HundredZeroProvider.PROVIDER)
        {

        }

        public HundredZeroSource(AbstractSource src)
            : this()
        {
            if (src.ID.HasValue)
                this.SetID(src.ID.Value);
            
            this.SetProviderID(src.ProviderID);
            this.SetSourceName(src.SourceName);
            this.SetMetaData(src.GetMetaData());
        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
