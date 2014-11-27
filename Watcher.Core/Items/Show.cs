using System;

namespace Watcher.Core.Items
{
    public class Show : AbstractItem
    {
        public Show(AbstractSource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null) 
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
