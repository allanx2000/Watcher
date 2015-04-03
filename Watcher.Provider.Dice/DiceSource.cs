using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.Dice
{
    class DiceSource : AbstractSource
    {
        
        public DiceSource(string name)
            : base(name, DiceProvider.PROVIDER)
        {

        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
