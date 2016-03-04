using System.Collections.Generic;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Core.Loaders
{

    public class SuperProvidersLoader : IProviderLoader
    {
        //private readonly V1ProvidersLoader v1;
        private readonly V2ProvidersLoader v2;
        
        public SuperProvidersLoader(string dllFolder)
        {
            //v1 = new V1ProvidersLoader(dllFolder);
            v2 = new V2ProvidersLoader(dllFolder);
        }

        /// <summary>
        /// Loads and returns the providers in the folder
        /// </summary>
        /// <returns></returns>
        public List<IProvider> GetProviders()
        {
            List<IProvider> providers = new List<IProvider>();

            providers.AddRange(v2.GetProviders());

            /*
            var v1Providers = v1.GetProviders();

            foreach (var p in v1Providers)
            {
                var converted = V1Converter.ConvertProvider(p);

                providers.Add(converted);
            }
            */

            return providers;
        }
    }
}
