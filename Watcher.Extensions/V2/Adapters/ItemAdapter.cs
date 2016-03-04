using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions.V2.Adapters
{
    [Obsolete]
    public class ItemAdapter : AbstractItem
    {
        private Extensions.V1.AbstractItem i;

        public ItemAdapter(Extensions.V1.AbstractItem i, AbstractSource src) : base(src, i.Name, i.ActionContent, i.ID, i.New, i.AddedDate)
        {
            this.i = i;
        }

        internal Extensions.V1.AbstractItem GetOldItem()
        {
            return i;
        }

    }
}
