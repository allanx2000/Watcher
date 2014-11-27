using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Core.Loaders
{
    public class AbstractDLLLoader<T>
    {
        enum TypeOfT
        {
            Interface,
            Class
        }
        
        private readonly string folder;
        private readonly string extension = ".dll";
        private readonly TypeOfT type;

        private Type typeInfo;

        public AbstractDLLLoader(string folder, string extension = ".dll")
        {
            this.folder = folder;
            this.extension = extension;

            typeInfo = typeof(T);

            if (typeInfo.IsInterface)
                type = TypeOfT.Interface;
            else
                type = TypeOfT.Class;
        }

        protected List<T> LoadFiles()
        {

            List<T> items = new List<T>();

            var files = from f in Directory.GetFiles(folder, "*" + extension) select new FileInfo(f);

            foreach (var f in files)
            {
                try
                {
                    //Assembly assem = Assembly.LoadFile(f.FullName);
                    Assembly assem = Assembly.LoadFrom(f.FullName);
                    
                    foreach (Type t in assem.GetTypes())
                    {
                        switch (type)
                        {
                            case TypeOfT.Class:
                                if (t.IsSubclassOf(typeInfo))
                                    items.Add(CreateInstance(assem, t));
                                break;
                            case TypeOfT.Interface:
                                if (t.GetInterface(typeInfo.FullName, true) != null)
                                    items.Add(CreateInstance(assem, t));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }


            return items;
        }

        private static T CreateInstance(Assembly assem, Type t)
        {
            return (T)assem.CreateInstance(t.FullName);
        }
    }
}
