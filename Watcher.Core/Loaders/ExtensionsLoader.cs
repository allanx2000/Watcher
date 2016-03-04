using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Watcher.Extensions;
using Watcher.Interop;

namespace Watcher.Core.Loaders
{
    public class ExtensionsLoader : IProviderLoader
    {

        private DirectoryInfo extensionsDirectory;

        private List<IDataStore> dataStores = new List<IDataStore>();
        private List<IProvider> providers = new List<IProvider>();

        public ExtensionsLoader(string extensionsPath)
        {
            try
            {
                Type providerType = typeof(IProvider);
                Type newProviderType = typeof(Watcher.Extensions.V2.AbstractProvider);
                
                if (Directory.Exists(extensionsPath))
                {
                    extensionsDirectory = new DirectoryInfo(extensionsPath);

                    FileInfo[] files = extensionsDirectory.GetFiles("*.dll");

                    foreach (FileInfo f in files)
                    {
                        try
                        {
                            Assembly assem = Assembly.LoadFile(f.FullName);

                            foreach (Type t in assem.GetTypes())
                            {
                                //TODO: Check this....
                                if (t.IsSubclassOf(providerType))
                                    providers.Add((IProvider) CreateInstance(assem, t));
                                /*
                                if (t.IsSubclassOf(newProviderType))
                                    providers.Add((Watcher.Extensions.V2.AbstractProvider)CreateInstance(assem, t));
                                else if (t.IsSubclassOf(providerType))
                                    V1Converter.ConvertProvider((AbstractProvider)CreateInstance(assem, t));

                                */
                            }
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private static object CreateInstance(Assembly assem, Type t)
        {
            return assem.CreateInstance(t.FullName);
        }

        public List<IProvider> GetProviders()
        {
            return providers;
        }

        public List<IDataStore> GetDataStores()
        {
            return dataStores;
        }
    }
}
