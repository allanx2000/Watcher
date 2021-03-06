﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Watcher.Extensions;

namespace Watcher.Core.Loaders
{

    public class V1ProvidersLoader : AbstractDLLLoader<AbstractProvider>
    {
        
        public V1ProvidersLoader(string dllFolder) : base(dllFolder)
        {}

        public List<AbstractProvider> GetProviders()
        {
            return base.LoadFiles();
        }
    }
}
