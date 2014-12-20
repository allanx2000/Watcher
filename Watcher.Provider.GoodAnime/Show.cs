using System;
using Watcher.Extensions;

namespace Watcher.Provider.GoodAnime
{
    public class Show : AbstractItem
    {
        public Show(AbstractSource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null) 
            : base(source, title, link, id, isNew, addedDate)
        {
        }
    }
}
