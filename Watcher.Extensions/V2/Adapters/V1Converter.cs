using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Watcher.Extensions.V2.Adapters
{
    public static class V1Converter
    {
        public static AbstractProvider ConvertProvider(Extensions.AbstractProvider provider)
        {
            return new ProvidersAdapter(provider);
        }

        public static GenericSource ConvertSource(Extensions.GenericSource s)
        {
            throw new NotImplementedException();
        }
    }
}
