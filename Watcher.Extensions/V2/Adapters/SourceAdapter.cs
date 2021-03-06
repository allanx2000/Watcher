﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V1 = Watcher.Extensions.V1;

namespace Watcher.Extensions.V2.Adapters
{ 
    [Obsolete]
    public class SourceAdapter : AbstractSource
    {
        private V1.AbstractSource src;
        private V1.AbstractProvider provider;

        internal V1.AbstractSource GetOldSource()
        {
            return src;
        }
        
        public static SourceAdapter CreateSourceAdapter(V1.AbstractSource src, V1.AbstractProvider provider)
        {
            string strDisabled = src.GetMetaDataValue(AbstractSource.DISABLED);
            bool disabled = strDisabled == null ? false : Convert.ToBoolean(strDisabled);

            GenericSource gs = new GenericSource(src.SourceName, src.ProviderID, disabled);
            
            //Update Meta
            Dictionary<string, MetaDataObject> newMeta = new Dictionary<string, MetaDataObject>();

            List<string> providerMeta = provider.GetMetaFields();
            {
                foreach (string name in providerMeta)
                {
                    var mdo = new MetaDataObject(name, name);
                    if (src.GetMetaData().ContainsKey(name))
                        mdo.SetValue(src.GetMetaDataValue(name));

                    newMeta.Add(mdo.ID, mdo);
                }
            }

            gs.SetMetaData(newMeta);
            
            if (src.ID.HasValue)
                gs.SetID(src.ID.Value);

            var sa = new SourceAdapter(gs);
            

            return sa;
        }

        //Already converted
        private SourceAdapter(GenericSource src) : base(src)
        {
            //this.provider = provider;
            this.SetMetaData(src.GetMetaData());

            this.SetProviderID(src.ProviderID);
            this.SetSourceName(src.SourceName);
            
        }
        
    }
}
