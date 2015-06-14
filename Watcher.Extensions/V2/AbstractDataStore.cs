using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Watcher.Extensions.Internal;

namespace Watcher.Extensions.V2
{
    /// <summary>
    /// Object wrapper for the underlying data source
    /// </summary>
    public abstract class AbstractDataStore
    {
        private readonly string type;

        private ObservableCollection<AbstractSource> sources = new MTObservableCollection<AbstractSource>();
        private ObservableCollection<AbstractItem> items = new MTObservableCollection<AbstractItem>();

        /// <summary>
        /// A list of default items, usually the latest N items
        /// </summary>
        public ObservableCollection<AbstractItem> Items
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
        public ObservableCollection<AbstractSource> Sources
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
        public void Initialize(List<AbstractProvider> providers)
        {

            Dictionary<string, AbstractProvider> providerLookup = providers.ToDictionary(p => p.GetProviderId());

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

                        //TODO: Is this necessary, why not just get it from the provider when needed?

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

                }

            }

            List<AbstractItem> its = LoadItems();

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
        protected abstract List<AbstractItem> LoadItems();


        public void RemoveSource(AbstractSource source)
        {
            bool removed = sources.Remove(source);

            if (removed)
            {
                RemoveFromDataStore(source);
            }
        }

        protected abstract void RemoveFromDataStore(AbstractSource source);

        public AbstractSource AddSource(AbstractSource source)
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
        protected abstract void DoInsertSource(AbstractSource source);


        public void UpdateSource(AbstractSource source)
        {
            DoUpdateSource(source);

            //Copy changes to active source

            var existing = Sources.FirstOrDefault(x => x.ID == source.ID);

            if (existing != null)
            {
                source.CopyTo(existing);
            }

        }

        protected abstract void DoUpdateSource(AbstractSource source);

        public void UpdateItem(List<AbstractItem> items)
        {
            foreach (var i in items)
            {
                UpdateItem(i);
            }
        }

        public abstract void UpdateItem(AbstractItem item);

        public List<AbstractItem> AddItems(List<AbstractItem> items)
        {
            List<AbstractItem> addedItems = new List<AbstractItem>();

            foreach (AbstractItem i in items)
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

        protected abstract bool DoAddItem(AbstractItem item);

        //public abstract void SaveItems();


        public abstract List<AbstractItem> Search(string filter);
    }
}
