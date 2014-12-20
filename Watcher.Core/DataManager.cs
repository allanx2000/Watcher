using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Watcher.Core.Items;
using Watcher.Extensions;
using Watcher.Extensions.Internal;

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

        struct UpdateParameters
        {
            public List<AbstractItem> AddedItems { get; set; }
            public int UpdateTimeOut { get; set; }
            public Action<List<AbstractItem>, string> Callback { get; set; }
            public List<Thread> WorkerThreads { get; set; }
        }

        public void UpdateItems(int updateTimeoutInMinutes = 2, Action<List<AbstractItem>, string> callback = null, bool multithread = true)
        {
            ClearMessages();

            List<AbstractItem> addedItems = new List<AbstractItem>();

            try
            {
                List<Thread> threads = new List<Thread>();
                //int threadCount = 0;


                foreach (AbstractSource s in DataStore.Sources)
                {
                    foreach (var p in providers)
                    {
                        if (!p.CanCheck(s)) continue;

                        if (multithread) //Add it as a thread
                            threads.Add(new Thread(() =>
                            {
                                DoItemsCheck(p, s, addedItems);

                                //threadCount--;
                            }));
                        else //Do it now
                        {
                            DoItemsCheck(p, s, addedItems);
                        }
                    }
                }


                if (multithread)
                {
                    //var forceEnd = DateTime.Now.AddMinutes(updateTimeoutInMinutes);

                    //threadCount = threads.Count;

                    UpdateParameters threadParams = new UpdateParameters()
                    {
                        Callback = callback,
                        UpdateTimeOut = updateTimeoutInMinutes,
                        WorkerThreads = threads,
                        AddedItems = addedItems
                    };

                    var th = new Thread(DoUpdates);

                    th.Start(threadParams);
                }
                else
                    callback.Invoke(addedItems, null);
            }
            catch (Exception e)
            {

                callback.Invoke(addedItems, e.Message);
            }
        }

        private void DoUpdates(object p)
        {
            if (!(p is UpdateParameters))
                throw new Exception("Parameter invalid");

            var paramz = (UpdateParameters)p;

            DateTime timeout = DateTime.Now.AddMinutes(paramz.UpdateTimeOut);
            //TODO: Get from prop.

            foreach (var t in paramz.WorkerThreads)
            {
                t.Start();
            }

            while (ThreadsRunning(paramz.WorkerThreads) > 0)
            {
                Thread.Sleep(2000);
                if (DateTime.Now > timeout)
                {
                    paramz.Callback.Invoke(paramz.AddedItems, "Timed Out!");
                    return;
                }
            }

            paramz.Callback.Invoke(paramz.AddedItems, null);

        }

        private int ThreadsRunning(List<Thread> threads)
        {
            int count = threads.FindAll(t => t.ThreadState != ThreadState.Stopped).Count;

            return count;
        }

        private MTObservableCollection<string> messages = new MTObservableCollection<string>();

        public MTObservableCollection<string> Messages
        {
            get
            {
                return messages;
            }
        }

        public void ClearMessages()
        {
            messages.Clear();
        }

        public void AddMessage(string message)
        {
            messages.Add(message);
        }

        private void DoItemsCheck(AbstractProvider p, AbstractSource s, List<AbstractItem> addedItems)
        {
            try
            {
                var results = p.CheckForNewItems(s);
                if (results != null)
                {
                    results = dataStore.AddItems(results);
                    addedItems.AddRange(results);
                }

                AddMessage(String.Format("Source: {0}, New Items: {1}",
                    s.GetDisplayName(), results == null ? 0 : results.Count));
            }
            catch (Exception e)
            {
                AddMessage(String.Format("Source: {0}, Error: {1}",
                    s.GetDisplayName(), e.Message));
            }
        }

    }
}
