using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Watcher.Core.Loaders
{
    public class ExtensionsLoader : IProviderLoader
    {

        private static ExtensionsLoader instance;

        private DirectoryInfo extensionsDirectory;

        private List<AbstractDataStore> dataStores = new List<AbstractDataStore>();
        private List<AbstractProvider> providers = new List<AbstractProvider>();

        public ExtensionsLoader(string extensionsPath)
        {
            try
            {
                Type providerType = typeof(AbstractProvider);
                Type dataStoreType = typeof(AbstractDataStore);

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
                                if (t.IsSubclassOf(providerType))
                                    providers.Add((AbstractProvider) CreateInstance(assem, t));
                           //     else if (t.IsSubclassOf(dataStoreType))
                           //         dataStores.Add((AbstractDataStore) CreateInstance(assem, t));
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

        public List<AbstractProvider> GetProviders()
        {
            return providers;
        }

        public List<AbstractDataStore> GetDataStores()
        {
            return dataStores;
        }
    }
}
