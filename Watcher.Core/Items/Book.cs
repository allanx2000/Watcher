using System;
using Watcher.Extensions;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Core.Items
{
    public class Book : AbstractItem
    {   
        public Book(ISource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null) 
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
