
using System;
using System.Collections.Generic;
using System.Threading;
using Watcher.Core.Items;

namespace Watcher.Core
{
    
    public class DataManager
    {
        private static DataManager _manager;

        public static void Initialize(AbstractDataStore dataStore, List<AbstractProvider> providers)
        {
            _manager = new DataManager(dataStore, providers);
        }

        public static DataManager Instance()
        {
                if (_manager == null)
                    throw new Exception("DataManager is unitialized");
                else return _manager;
        }

        //Instance
        AbstractDataStore dataStore;
        List<AbstractProvider> providers;

        private DataManager(AbstractDataStore dataStore, List<AbstractProvider> providers)
        {
            this.dataStore = dataStore;
            this.providers = providers;

            dataStore.Initialize(providers);
        }

        public AbstractDataStore DataStore
        {
            get
            {
                return dataStore;
            }
        }


        public List<AbstractProvider> GetProviders()
        {
            return providers;
        }

        public void UpdateBooks(int updateTimeoutInMinutes = 2, Action<List<AbstractItem>, string> callback = null, bool multithread = true)
        {
            List<Thread> threads = new List<Thread>();
            int threadCount = 0;

            List<AbstractItem> addedItems = new List<AbstractItem>();

            foreach (AbstractSource s in DataStore.Sources)
            {
                foreach (var p in providers)
                {
                    if (!p.CanCheck(s)) continue;
                    
                    if (multithread)
                        threads.Add(new Thread(() =>
                        {
                            DoItemsCheck(p, s, addedItems);
                                
                            threadCount--;
                        }));
                    else
                    {
                        DoItemsCheck(p, s, addedItems);
                    }
                }
            }

            if (multithread)
            {
                var forceEnd = DateTime.Now.AddMinutes(updateTimeoutInMinutes);

                threadCount = threads.Count;

                var th = new Thread(() =>
                {
                    foreach (var t in threads)
                    {
                        t.Start();
                    }

                    while (threadCount > 0 && DateTime.Now < forceEnd)
                    {
                        Thread.Sleep(5000);
                    }

                    callback.Invoke(addedItems, threadCount > 0? "Timed Out!" : null);
                });

                th.Start();
            }
            else
                callback.Invoke(addedItems, null);

            /*while (threadCount != 0)
            {
                Thread.Sleep(5000);
            }

            Console.WriteLine(addedItems.Count);*/
        }

        private void DoItemsCheck(AbstractProvider p, AbstractSource s, List<AbstractItem> addedItems)
        {
            try
            {
                var results = p.CheckForNewItems(s);
                if (results != null)
                {
                    addedItems.AddRange(dataStore.AddItems(results));
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
