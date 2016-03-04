using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Watcher.Extensions.Internal;
using Watcher.Interop;

namespace Watcher.Extensions.V2
{
    /// <summary>
    /// Object wrapper for the underlying data source
    /// </summary>
    public abstract class AbstractDataStore : IDataStore
    {
        private readonly string type;

        private ObservableCollection<ISource> sources = new MTObservableCollection<ISource>();
        private ObservableCollection<IDataItem> items = new MTObservableCollection<IDataItem>();

        /// <summary>
        /// A list of default items, usually the latest N items
        /// </summary>
        public ObservableCollection<IDataItem> Items
        {
            get
            {
                return items;
            }
            private set
            {
                items = value;
            }
        }

        /// <summary>
        /// A list of all sources in the datastore
        /// </summary>
        public ObservableCollection<ISource> Sources
        {
            get
            {
                return sources;
            }
            private set
            {
                sources = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">This usually is the ProviderId</param>
        protected AbstractDataStore(string type)
        {
            this.type = type;
        }

        /// <summary>
        /// DataStore identifier
        /// </summary>
        public string Type
        {
            get { return type; }
        }

        /// <summary>
        /// Initializes the data store for usage
        /// </summary>
        /// <param name="providers">A list of loaded providers</param>
        public void Initialize(List<IProvider> providers)
        {

            Dictionary<string, IProvider> providerLookup = providers.ToDictionary(p => p.GetProviderId());

            List<GenericSource> sources = LoadSources();

            List<int> sourceIds = new List<int>();

            foreach (var src in sources)
            {
                try
                {
                    if (providerLookup.ContainsKey(src.ProviderID))
                    {
                        //Use the provider to convert the GenericSource to Provider-specific Source, if defined
                        var provider = providerLookup[src.ProviderID];
                        var newSource = provider.CastSource(src);

                        var nsMD = newSource.GetMetaData();

                        //The metadata fields loaded from the datastore are not completely initialized (no FieldType, DisplayName, etc.)
                        //this gets the data from the Provider itself and updates the values in the source

                        foreach (var md in provider.GetMetaFields())
                        {   
                            if (newSource.GetMetaData().ContainsKey(md.ID))
                            {
                                nsMD[md.ID].SetDisplayName(md.DisplayName);
                                nsMD[md.ID].SetFieldType(md.FieldType);
                                nsMD[md.ID].SetSelectorValues(md.SelectorValues);
                            }
                        }

                        Sources.Add(newSource);

                        sourceIds.Add(src.ID.Value);
                    }
                }
                catch (Exception e)
                {
                    //Error loading source, just skip
                }
            }

            List<IDataItem> its = LoadItems();

            //Only load the ones that have a Source loaded
            foreach (AbstractItem i in its)
            {
                if (sourceIds.Contains(i.SourceId))
                {
                    Items.Add(i);
                }
            }
        }

        private void RunEviction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the sources in the data store
        /// </summary>
        /// <returns></returns>
        protected abstract List<GenericSource> LoadSources();

        /// <summary>
        /// Returns the default items from the data store
        /// </summary>
        /// <returns></returns>
        protected abstract List<IDataItem> LoadItems();


        public void RemoveSource(ISource source)
        {
            bool removed = sources.Remove(source);

            if (removed)
            {
                RemoveFromDataStore(source);
            }
        }

        protected abstract void RemoveFromDataStore(ISource source);

        public ISource AddSource(ISource source)
        {
            if (sources.Contains(source))
                throw new Exception("Source already exists.");
            else if (source.ID != null)
                throw new Exception("Source should not have ID");
            else
            {
                try
                {
                    sources.Add(source);

                    DoInsertSource(source);

                    if (source.ID == null)
                        throw new Exception("Logic error, expected DataStore to set ID");
                }
                catch (Exception e)
                {
                    sources.RemoveAt(sources.Count - 1);
                    throw;
                }

                return source;
            }
        }

        /// <summary>
        /// Insert the Source into the actual data store.
        /// This function should set the ID on the source object
        /// </summary>
        /// <param name="source"></param>
        protected abstract void DoInsertSource(ISource source);


        public void UpdateSource(ISource source)
        {
            DoUpdateSource(source);

            //Copy changes to active source

            var existing = Sources.FirstOrDefault(x => x.ID == source.ID);

            if (existing != null)
            {
                source.CopyTo(existing);
            }

        }

        protected abstract void DoUpdateSource(ISource source);

        public void UpdateItem(List<IDataItem> items)
        {
            foreach (var i in items)
            {
                UpdateItem(i);
            }
        }

        public abstract void UpdateItem(IDataItem item);

        public List<IDataItem> AddItems(List<IDataItem> items)
        {
            List<IDataItem> addedItems = new List<IDataItem>();

            foreach (IDataItem i in items)
            {
                try
                {
                    if (DoAddItem(i))
                    {
                        Items.Add(i);
                        addedItems.Add(i);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Could not add: {0}; Message: {1}", i.Name, e.Message), e);
                }
            }

            return addedItems;
        }

        protected abstract bool DoAddItem(IDataItem item);

        //public abstract void SaveItems();


        public abstract List<IDataItem> Search(string filter);
    }
}
