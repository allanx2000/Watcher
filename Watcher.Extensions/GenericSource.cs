using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Extensions
{
    public class GenericSource : AbstractSource
    {
        public GenericSource(string sourceName, string providerId)
            : base(sourceName, providerId)
        {
        }

    }
}
