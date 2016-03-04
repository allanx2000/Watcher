using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Watcher.Interop;

namespace Watcher.Core.Loaders
{
    public class AbstractDLLLoader
    {
        private readonly string folder;
        private readonly string extension = ".dll";

        public AbstractDLLLoader(string folder, string extension = ".dll")
        {
            this.folder = folder;
            this.extension = extension;
        }

        protected List<IProvider> LoadFiles()
        {

            Type intf = typeof(IProvider);

            List<IProvider> items = new List<IProvider>();

            var files = from f in Directory.GetFiles(folder, "*" + extension) select new FileInfo(f);

            foreach (var f in files)
            {
                try
                {
                    //Assembly assem = Assembly.LoadFile(f.FullName);
                    Assembly assem = Assembly.LoadFrom(f.FullName);

                    foreach (Type t in assem.GetTypes())
                    {
                        if (t.GetInterface(intf.FullName, true) != null)
                            items.Add(CreateInstance(assem, t));
                    }
                }
                catch (Exception e)
                {

                }
            }


            return items;
        }

        private static IProvider CreateInstance(Assembly assem, Type t)
        {
            return (IProvider)assem.CreateInstance(t.FullName);
        }
    }
}
