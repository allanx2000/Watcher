using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V1 = Watcher.Extensions;

namespace Watcher.Extensions.V2.Adapters
{
    public class V2toV1ItemAdapter : V1.AbstractItem
    {
        private AbstractItem item;

        public V2toV1ItemAdapter(AbstractItem item, V1.AbstractSource src) : base(src,
            item.Name, item.ActionContent, item.ID, item.New, item.AddedDate)
        {
            this.item = item;
        }

        public AbstractItem GetItem()
        {
            return item;
        }

    }
}
