using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Provider.GoodAnime
{
    class GoodAnimeSource : AbstractSource
    {
        public GoodAnimeSource() 
            : base("GoodAnime", GoodAnimeProvider.PROVIDER)
        {

        }

        public GoodAnimeSource(GenericSource src) : base(src)
        {

        }

        


        public override string GetDisplayName()
        {
            return SourceName;
        }
    }
}
