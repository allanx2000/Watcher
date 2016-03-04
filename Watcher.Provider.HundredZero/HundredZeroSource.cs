using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;
using Watcher.Extensions.V2;

namespace Watcher.Provider.HundredZero
{
    class HundredZeroSource : AbstractSource
    {        
        public HundredZeroSource(string name)
            : base(name, HundredZeroProvider.PROVIDER)
        {

        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
