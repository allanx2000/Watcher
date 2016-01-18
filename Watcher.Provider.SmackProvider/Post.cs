using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;

namespace Watcher.Provider.Smack
{
    public class Post : AbstractItem
    {
        public Post(AbstractSource source, string title, string link, int? id = null, bool isNew = true,
            DateTime? addedDate = null)
            : base(source, title, link, id, isNew, addedDate)
        {

        }
    }
}
