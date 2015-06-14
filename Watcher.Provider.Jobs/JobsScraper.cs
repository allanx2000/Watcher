using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Jobs
{
    class JobsScraper
    {
        internal static List<AbstractItem> GetFromDice(Extensions.V2.AbstractSource source)
        {
            throw new NotImplementedException();
        }

        internal static List<AbstractItem> GetFromMonster(Extensions.V2.AbstractSource source)
        {
            return MonsterProvider.GetNewItems(source);
        }
    }
}
