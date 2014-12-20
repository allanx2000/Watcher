using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.HundredZero
{
    class HundredZeroSource : AbstractSource
    {        
        public HundredZeroSource() 
            : base("HundredZero", HundredZeroProvider.PROVIDER)
        {

        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
