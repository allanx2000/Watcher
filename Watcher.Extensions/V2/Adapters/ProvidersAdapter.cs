using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using V1 = Watcher.Extensions.V1;

namespace Watcher.Extensions.V2.Adapters
{
    [Obsolete]
    public class ProvidersAdapter : AbstractProvider
    {
        private V1.AbstractProvider provider;
        
        public ProvidersAdapter(V1.AbstractProvider provider) : base(provider.ProviderId)
        {
            this.provider = provider;

            var so = provider.GetSourceOptions();

            this.HasUniqueName = so.HasUniqueName;
            this.HasUrlField = so.HasURLField;
        }

        public override List<MetaDataObject> GetMetaFields()
        {
            //TODO: Feels like code already exists somewhere... Probably Generic Source or Source Adapter

            List<MetaDataObject> mds = new List<MetaDataObject>();

            foreach (string id in provider.GetMetaFields())
            {
                mds.Add(new MetaDataObject(id, id, MetaDataObject.Type.String));
            }

            return mds;
        }

        protected override AbstractSource DoCreateNewSource(string name, string url, List<MetaDataObject> metaData)
        {
            Dictionary<string,string> oldMetaFormat = ConvertToOldFormat(metaData);

            V1.AbstractSource src = provider.CreateNewSource(name, url, oldMetaFormat);

            return SourceAdapter.CreateSourceAdapter(src, provider);
        }

        private Dictionary<string, string> ConvertToOldFormat(IEnumerable<MetaDataObject> metaData)
        {
            List<MetaDataObject> list = metaData is List<MetaDataObject>? 
                (List<MetaDataObject>) metaData :
                new List<MetaDataObject>(metaData);

            return ConvertToOldFormat(list);
        }

        private Dictionary<string, string> ConvertToOldFormat(List<MetaDataObject> metaData)
        {
            Dictionary<string, string> old = new Dictionary<string, string>();

            foreach (MetaDataObject mdo in metaData)
            {
                old.Add(mdo.ID, mdo.Value == null? null : mdo.Value.ToString());
            }

            return old;
        }

        protected override List<AbstractItem> GetNewItems(AbstractSource source)
        {
            V1.AbstractSource oldSource = ConvertToOldFormat(source);

            var items = provider.SupportGetNewItems(oldSource);

            List<AbstractItem> newItems = new List<AbstractItem>();

            foreach (var i in items)
            {
                newItems.Add(new ItemAdapter(i, source));
            }

            return newItems;
            
        }

        private V1.AbstractSource ConvertToOldFormat(AbstractSource source)
        {
            if (source is SourceAdapter)
            {
                return ((SourceAdapter)source).GetOldSource();
            }
            else
            {
                V1.AbstractSource src = new V1.GenericSource(source.SourceName, source.ProviderID);
                src.SetID(source.ID.Value);

                var md = ConvertToOldFormat(source.GetMetaData().Values);
                src.SetMetaData(md);

                return src;
            }
        }

        public override AbstractSource CastSource(GenericSource src)
        {
            var newFormat = base.CastSource(src);

            //Add the new format that are missing
            var providerMD = this.GetMetaFields();
            Dictionary<string, MetaDataObject> sourceMD = newFormat.GetMetaData(); 

            foreach (var md in providerMD)
            {
                if (!newFormat.GetMetaData().ContainsKey(md.ID))
                    sourceMD.Add(md.ID, md);
            }


            return newFormat;
        }

        public override void DoAction(AbstractItem item)
        {
            V1.AbstractItem i = ConvertToOldFormat(item);

            provider.DoAction(i);
            
        }

        private V1.AbstractItem ConvertToOldFormat(AbstractItem item)
        {
            if (item is ItemAdapter)
            {
                return ((ItemAdapter)item).GetOldItem();
            }
            else
            {
                //TODO: Should cache these converts...
                var src = ConvertToOldFormat(item.GetSource());
                var adapter = new V2toV1ItemAdapter(item, src);
                return adapter;
            }
        }

        
    }
}
