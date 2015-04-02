using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.TED
{
    class TEDSource : AbstractSource
    {
        public TEDSource(string name)
            : base(name, TEDProvider.PROVIDER)
        {

        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
