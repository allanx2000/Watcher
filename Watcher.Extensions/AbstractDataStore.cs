using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Watcher.Extensions;
using Watcher.Extensions.Internal;

namespace Watcher.Extensions
{
    public abstract class AbstractDataStore
    {
        private readonly string type;

        private ObservableCollection<AbstractSource> sources = new MTObservableCollection<AbstractSource>();
        private ObservableCollection<AbstractItem> items = new MTObservableCollection<AbstractItem>();


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

        protected AbstractDataStore(string type)
        {
            this.type = type;
        }

        public string Type
        {
            get { return type; }
        }
        
        public void Initialize(List<AbstractProvider> providers)
        {

            Dictionary<string, AbstractProvider> providerLookup = providers.ToDictionary(p => p.GetProviderId());

            List<GenericSource> sources = LoadSources();
            
            List<int> sourceIds = new List<int>();

            foreach (var src in sources)
            {
                if (providerLookup.ContainsKey(src.ProviderID))
                {
                    Sources.Add(providerLookup[src.ProviderID].CastSource(src));
                    sourceIds.Add(src.ID.Value);
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

        protected abstract List<GenericSource> LoadSources();

        protected abstract List<AbstractItem> LoadItems();


        public void RemoveSource(AbstractSource source)
        {
            bool removed = sources.Remove(source);

            if (removed)
            {
                RemoveFromDataStore(source);
            }
        }

        public abstract void RemoveFromDataStore(AbstractSource source);

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
