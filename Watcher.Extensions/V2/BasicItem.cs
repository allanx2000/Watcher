using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Interop;

namespace Watcher.Extensions.V2
{
    //Basic Implementation of AbstractItem
    public class BasicItem : AbstractItem
    {
        public BasicItem(ISource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null)
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
