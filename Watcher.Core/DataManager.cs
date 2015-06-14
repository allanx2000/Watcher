using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Watcher.Core.Items;
using Watcher.Extensions.V2;
using Watcher.Extensions.Internal;

namespace Watcher.Core
{

    public class DataManager
    {
        private static DataManager _manager;

        /// <summary>
        /// Initialize the DataManager for use
        /// </summary>
        /// <param name="dataStore"></param>
        /// <param name="providers"></param>
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
            public Action<bool, List<AbstractItem>, object> Callback { get; set; }
            public List<Thread> WorkerThreads { get; set; }
        }

        private List<Thread> currentThreads = new List<Thread>();
        private Thread mainUpdateThread = null;

        /// <summary>
        /// Aborts the update, only works if multithreading
        /// </summary>
        public void AbortUpdate()
        {
            foreach (Thread t in currentThreads)
            {
                if (t.IsAlive)
                {
                    t.Abort();
                    AddMessage(t.Name + "was aborted");
                }
            }

            if (mainUpdateThread != null
                && mainUpdateThread.IsAlive)
            {
                mainUpdateThread.Abort();
            }
        }

        /// <summary>
        /// Gets updates from the loaded Sources using their Providers
        /// </summary>
        /// <param name="updateTimeoutInMinutes"></param>
        /// <param name="callback"></param>
        /// <param name="multithread"></param>
        public void UpdateItems(int updateTimeoutInMinutes = 2, Action<bool, List<AbstractItem>, object> callback = null, bool multithread = true)
        {
            //multithread = false;

            ClearMessages();

            AddMessage("Updating...");

            List<AbstractItem> addedItems = new List<AbstractItem>();

            try
            {
                List<Thread> threads = new List<Thread>();
                
                //Looks for a the provider that can handle it and calls it
                foreach (AbstractSource s in DataStore.Sources)
                {
                    foreach (var p in providers)
                    {
                        if (!p.CanCheck(s)) continue;


                        if (multithread) //Add it as a thread
                        {
                            Thread worker = new Thread(() =>
                            {
                                DoItemsCheck(p, s, addedItems);
                            });

                            worker.Name = s.GetDisplayName();

                            threads.Add(worker);
                        }
                        else //Do it now
                        {
                            DoItemsCheck(p, s, addedItems);
                        }
                    }
                }


                if (multithread)
                {
                    UpdateParameters threadParams = new UpdateParameters()
                    {
                        Callback = callback,
                        UpdateTimeOut = updateTimeoutInMinutes,
                        WorkerThreads = threads,
                        AddedItems = addedItems
                    };

                    mainUpdateThread = new Thread(DoUpdates);

                    mainUpdateThread.Start(threadParams);

                    currentThreads = threads;
                }
                else
                    callback.Invoke(true, addedItems, null);
            }
            catch (Exception e)
            {

                callback.Invoke(false, addedItems, e.Message);
            }
        }

        private void DoUpdates(object p)
        {
            DateTime start = DateTime.Now;

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
                    paramz.Callback.Invoke(false, paramz.AddedItems, "Timed Out!");
                    return;
                }
            }

            DateTime end = DateTime.Now;

            TimeSpan ts = end - start;

            paramz.Callback.Invoke(true, paramz.AddedItems, ts);

        }

        private int ThreadsRunning(List<Thread> threads)
        {
            int count = threads.FindAll(t => t.ThreadState != ThreadState.Stopped).Count;

            return count;
        }

        #region Update Messages
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
        #endregion

        /// <summary>
        /// Actual logic for checking for updates.
        /// This is usually run from separate threads by UpdateItems
        /// </summary>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <param name="addedItems"></param>
        private void DoItemsCheck(AbstractProvider p, AbstractSource s, List<AbstractItem> addedItems)
        {
            try
            {
                DateTime start = DateTime.Now;

                var results = p.CheckForNewItems(s);

                if (results != null)
                {
                    results = dataStore.AddItems(results);
                    addedItems.AddRange(results);
                }

                DateTime end = DateTime.Now;

                TimeSpan ts = end - start;

                AddMessage(String.Format("Source: {0}, New Items: {1}, Total Time: {2}",
                    s.GetDisplayName(), 
                    results == null ? 0 : results.Count,
                    (int) ts.TotalSeconds + " seconds"));
            }
            catch (Exception e)
            {
                AddMessage(String.Format("Source: {0}, Error: {1}",
                    s.GetDisplayName(), e.Message));
            }
        }

    }
}
