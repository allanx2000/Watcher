using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Jobs
{
    class JobItem : AbstractItem
    {
        public JobItem(AbstractSource source, string name, string link) : base(source, name,link)
        {
        }
    }
}
