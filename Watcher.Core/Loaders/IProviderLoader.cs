using System.Collections.Generic;
using Watcher.Interop;

namespace Watcher.Core.Loaders
{
    public interface IProviderLoader
    {   
        List<IProvider> GetProviders();
    }
}
