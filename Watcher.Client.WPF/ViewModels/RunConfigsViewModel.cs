using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Watcher.Core;
using Watcher.DataStore.SQLite;

namespace Watcher.Client.WPF.ViewModels
{
    public class RunConfigsViewModel : ViewModel
    {
        private string providersPath;
        private string dataStorePath;
        private string dataStoreKey = SQLiteDataStore.DATA_STORE_TYPE;
        private int updateFrequency;
        private int updateTimeout;

        private Properties.Settings AppConfig = Properties.Settings.Default;
        
        public RunConfigsViewModel()
        {
            ProvidersPath = AppConfig.ProvidersPath;
            DataStorePath = AppConfig.DataStoreFile;
            UpdateFrequency = AppConfig.UpdateFrequency;
            UpdateTimeout = AppConfig.UpdateTimeout;


        }

        public void SaveConfigs()
        {
            

            AppConfig.DataStoreKey = dataStoreKey;

            ValidateValues();
            
            AppConfig.ProvidersPath = ProvidersPath;
            AppConfig.DataStoreFile = DataStorePath;
            AppConfig.UpdateFrequency = UpdateFrequency;
            AppConfig.UpdateTimeout = UpdateTimeout;

            AppConfig.Save();
        }

        private void ValidateValues()
        {
            try
            {
                FileInfo f = new FileInfo(ProvidersPath);
            }
            catch
            {
                throw new Exception("ProvidersPath is not a valid.");

            }

            if (!Directory.Exists(ProvidersPath))
                throw new Exception("ProviderPath does not exist");

            
            try
            {
                FileInfo f = new FileInfo(DataStorePath);
            }
            catch
            {
                throw new Exception("DataStorePath is not a valid.");
            }

            if (UpdateFrequency <= 0)
                throw new Exception("UpdateFrequency must be greater than 0");

            if (updateTimeout < 2)
                throw new Exception("UpdateTimeout must be greater than 2 minutes");
        }
        
        #region Properties


        public int UpdateFrequency
        {
            get
            {
                return updateFrequency;
            }
            set
            {
                updateFrequency = value;
                RaisePropertyChanged("UpdateFrequency");
            }
        }

        public int UpdateTimeout
        {
            get
            {
                return updateTimeout;
            }
            set
            {
                updateTimeout = value;
                RaisePropertyChanged("UpdateTimeout");
            }
        }

        public string ProvidersPath
        {
            get { return providersPath; }
            set
            {
                providersPath = value;

                RaisePropertyChanged("ProvidersPath");
            }
        }

        public string DataStorePath
        {
            get { return dataStorePath; }
            set
            {
                dataStorePath = value;
                
                RaisePropertyChanged("DataStorePath");
            }
        }

        #endregion


    }
}
