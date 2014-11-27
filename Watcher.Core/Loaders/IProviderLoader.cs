using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Core.Loaders
{
    public interface IProviderLoader
    {   List<AbstractProvider> GetProviders();
    }
}
