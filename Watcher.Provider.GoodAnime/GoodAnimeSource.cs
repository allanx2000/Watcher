using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Core;

namespace Watcher.Provider.GoodAnime
{
    class GoodAnimeSource : AbstractSource
    {
        public GoodAnimeSource() 
            : base("GoodAnime", GoodAnimeProvider.PROVIDER)
        {

        }

        public GoodAnimeSource(AbstractSource src) :this()
        {
            if (src.ID.HasValue)
                this.SetID(src.ID.Value);
            
            this.SetProviderID(src.ProviderID);
            this.SetSourceName(src.SourceName);
            this.SetMetaData(src.GetMetaData());
            //this.SetURL(src.URL);
        }


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
