using System;
using Watcher.Extensions.Internal;

namespace Watcher.Extensions.V1
{
    [Obsolete]
    public abstract class AbstractItem
    {
        public class GenericItem : AbstractItem
        {
            public GenericItem(int id, AbstractSource source, string title, string meta, bool isNew,
                DateTime addedDate)
                : base(source, title, meta, id, isNew, addedDate)
            {

            }
        }

        private readonly AbstractSource source;

        public AbstractSource GetSource()
        {
            return source;
        }

        public static AbstractItem CreateGenericItem(int id, AbstractSource source, string title, string meta, bool isNew,
            DateTime addedDate)
        {
            return new GenericItem(id, source, title, meta, isNew, addedDate);
        }

        protected AbstractItem(AbstractSource source, string title, string link, int? id = null, bool isNew = true, DateTime? addedDate = null)
        {
            this.source = source;

            this.SourceId = source.ID.Value;
            this.SourceName = source.SourceName;
            this.Provider = source.ProviderID;
            
            this.Name = title;
            this.ActionContent = link;

            this.AddedDate = !addedDate.HasValue? DateTime.Now : addedDate.Value;

            this.ID = id;
            this.New = isNew;
        }

        public bool New { get; private set; }
        
        public int? ID { get; private set; }
        public string Name { get; private set; }
        public string ActionContent { get; private set; }

        public int SourceId { get; private set; }

        public DateTime AddedDate { get; private set; }

        public AbstractItem SetId(int id)
        {
            this.ID = id;

            return this;
        }

        public AbstractItem SetNew(bool isNew)
        {
            New = isNew;
            return this;
        }

        
        public override bool Equals(object obj)
        {
            var item = obj as AbstractItem;

            if (item != null)
            {
                return item.ActionContent == this.ActionContent;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return ActionContent.GetHashCode();
        }

        public string SourceName { get; set; }

        public string Provider { get; set; }
    }
}
