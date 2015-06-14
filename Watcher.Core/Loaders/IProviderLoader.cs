﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions.V2;

namespace Watcher.Core.Loaders
{
    public interface IProviderLoader
    {   
        List<AbstractProvider> GetProviders();
    }
}
