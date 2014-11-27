using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watcher.Client.WPF
{
    public class CurrentConfigurations
    {
        public CurrentConfigurations()
        {
            var properties = Properties.Settings.Default;
            ProvidersPath = properties.ProvidersPath;
            DataStorePath = properties.DataStoreFile;
            DataStoreKey = properties.DataStoreKey;
            UpdateFrequency = properties.UpdateFrequency;
            UpdateTimeOut = properties.UpdateTimeout;
        }

        public void SaveConfigs()
        {
            //Save to properties
            var properties = Properties.Settings.Default;

            properties.DataStoreFile = DataStorePath;
            properties.DataStoreKey = DataStoreKey;
            properties.ProvidersPath = ProvidersPath;
            properties.UpdateTimeout = UpdateTimeOut;
            properties.UpdateFrequency = UpdateFrequency;
            
            properties.Save();

        }

        public int UpdateFrequency { get; set; }

        public int UpdateTimeOut { get; set; }


        public string ProvidersPath { get; set; }

        //public string ExtensionsPath { get; set; }

        public string DataStoreKey { get; set; }

        public string DataStorePath { get; set; }


        public bool FirstRun
        {
            get
            {
                return String.IsNullOrEmpty(DataStorePath)
                    || String.IsNullOrEmpty(DataStoreKey)
                    || String.IsNullOrEmpty(ProvidersPath);
            }
        }
    }
}
