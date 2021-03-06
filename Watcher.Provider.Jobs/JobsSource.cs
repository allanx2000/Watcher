﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Provider.Jobs
{
    class JobsSource : AbstractSource
    {
        
        public JobsSource(string name) : base(name, JobsProvider.PROVIDER)
        {

        }

        public string Location
        {
            get
            {
                return MetaDataObject.FindIn(GetMetaData(), JobsProvider.META_LOCATION).GetValueAsString();
            }
            set
            {
                SetMetaDataValue(JobsProvider.META_LOCATION, value);
            }
        }

        public string Source
        {
            get
            {
                return MetaDataObject.FindIn(GetMetaData(), JobsProvider.META_SOURCE).GetValueAsString();
            }
            set
            {
                SetMetaDataValue(JobsProvider.META_SOURCE, value);
            }
        }

        public string Query
        {
            get
            {
                return MetaDataObject.FindIn(GetMetaData(),JobsProvider.META_QUERY).GetValueAsString();
            }
            set
            {
                SetMetaDataValue(JobsProvider.META_QUERY, value);
            }
        }

        public override string GetDisplayName()
        {
            return string.Format("{0} > {1}: {2}", ProviderID, Source, SourceName);
        }

        
        internal static JobsSource CreateFrom(ISource source)
        {
            if (source is JobsSource)
                return (JobsSource) source;

            JobsSource js = new JobsSource(source.SourceName);
            js.SetMetaData(source.GetMetaData()); //Only has non-protected
            js.SetID(source.ID.Value);

            if (source is ISource2)
                js.Services = ((ISource2)source).Services;

            return js;
        }
    }
}
