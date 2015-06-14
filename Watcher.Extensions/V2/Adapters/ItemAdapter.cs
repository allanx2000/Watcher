using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions.V2.Adapters
{
    public class ItemAdapter : AbstractItem
    {
        private Extensions.AbstractItem i;

        public ItemAdapter(Extensions.AbstractItem i, AbstractSource src) : base(src, i.Name, i.ActionContent, i.ID, i.New, i.AddedDate)
        {
            this.i = i;
        }

        internal Extensions.AbstractItem GetOldItem()
        {
            return i;
        }

    }
}
