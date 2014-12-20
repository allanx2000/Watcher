using System;
using Watcher.Extensions;

namespace Watcher.Provider.HundredZero
{
    public class Book : AbstractItem
    {   
        public Book(AbstractSource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null) 
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
