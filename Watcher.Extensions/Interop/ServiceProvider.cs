using System.Collections.Generic;
using System.Linq;

namespace Watcher.Interop
{
    public class ServiceProvider
    {
        private IDataStore dataStore;
        public ServiceProvider(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public bool ItemExists(IDataItem item)
        {
            var matches = dataStore.Search(item.Name);
            if (matches == null)
                return false;

            foreach (var m in matches)
            {
                if (m.Provider == item.Provider
                    && m.ActionContent == item.ActionContent)
                    return true;
            }

            return false;
        }
    }
}