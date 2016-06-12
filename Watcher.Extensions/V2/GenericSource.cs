using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Extensions.V2
{
    public class GenericSource : AbstractSource
    {
        public GenericSource(string sourceName, string providerId, bool disabled)
            : base(sourceName, providerId, disabled)
        {
        }
        
    }
}
