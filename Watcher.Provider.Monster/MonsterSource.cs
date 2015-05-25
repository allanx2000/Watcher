using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.Monster
{
    class MonsterSource : AbstractSource
    {

        public MonsterSource(string name)
            : base(name, MonsterProvider.PROVIDER)
        {

        }

        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
