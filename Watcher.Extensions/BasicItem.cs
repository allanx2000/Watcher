using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions
{
    //Basic Implementation of AbstractItem
    public class BasicItem : AbstractItem
    {
        public BasicItem(AbstractSource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null)
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
