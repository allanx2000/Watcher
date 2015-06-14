using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions.V2;

namespace Watcher.Core.Loaders
{

    public class V2ProvidersLoader : AbstractDLLLoader<AbstractProvider>, IProviderLoader
    {
        
        public V2ProvidersLoader(string dllFolder) : base(dllFolder)
        {}

        public List<AbstractProvider> GetProviders()
        {
            return base.LoadFiles();
        }
    }
}
