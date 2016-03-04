using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Core.Loaders
{

    public class V2ProvidersLoader : AbstractDLLLoader, IProviderLoader
    {
        
        public V2ProvidersLoader(string dllFolder) : base(dllFolder)
        {}

        public List<IProvider> GetProviders()
        {
            return base.LoadFiles();
        }
    }
}
