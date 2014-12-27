using Innouvous.Utils.DialogWindow.Windows;
using Innouvous.Utils.DialogWindow.Windows.Components;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Watcher.Client.WPF.ViewModels;
using Watcher.Core;
using Watcher.Core.Loaders;

namespace Watcher.Client.WPF
{




    /// <summary>
    /// Interaction logic for DSSelector.xaml
    /// </summary>
    public partial class RunConfigsWindow : Window
    {
        private Properties.Settings AppConfig = Properties.Settings.Default;
        
        public bool Cancelled
        { get; set; }

        public bool IsFirstRun
        {
            get
            {
                return String.IsNullOrEmpty(AppConfig.DataStoreFile)
                    || String.IsNullOrEmpty(AppConfig.ProvidersPath);
            }
        }

        private const string DataPath = "DataPath";
        private const string ProvidersPath = "ProvidersPath";
        private const string UpdateFrequency = "UpdateFrequency";
        private const string UpdateTimeOut = "UpdateTimeOut";

        public RunConfigsWindow()
        {
            InitializeComponent();

            List<ValueComponent> fields = new List<ValueComponent>()
            {
                PathSelectComponent.SaveFileComponent(
                    new ComponentArgs() 
                    { 
                        DisplayName = "Data File Path", 
                        FieldName=DataPath,
                        InitialData=AppConfig.DataStoreFile,
                    }, 
                    ext: PathSelectComponent.MakeExtension("SQLite DB", "*.sqlite"), 
                    confirmOverwrite: false),
                PathSelectComponent.SelectFolderComponent(
                    new ComponentArgs()
                    {
                        DisplayName = "Providers Path",
                        FieldName = ProvidersPath,
                        InitialData = AppConfig.ProvidersPath
                    }),
                new TextBoxComponent(
                    new ComponentArgs()
                    {
                        DisplayName = "Update Frequency (min)",
                        FieldName = UpdateFrequency,
                        InitialData = AppConfig.UpdateFrequency
                    }, maxLength: 3),
                new TextBoxComponent(    
                    new ComponentArgs()
                    {
                        DisplayName = "Update Timeout (min)",
                        FieldName = UpdateTimeOut,
                        InitialData = AppConfig.UpdateTimeout
                    }, TextBoxComponent.FieldType.Integer, 3)
            };

            var options = DialogControlOptions.SetDataInputOptions(fields);
            DlgControl.SetupControl(options);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
        
                //configsViewModel.SaveConfigs();


                var data = DlgControl.GetOptionsData();

                foreach (KeyValuePair<string, object> kv in data)
                {
                    
                    switch (kv.Key)
                    {
                        //TODO: Generic File/Folder/Path Checks
                        case DataPath:
                            new FileInfo(kv.Value.ToString());
                            AppConfig.DataStoreFile = kv.Value.ToString();
                            break;
                        case ProvidersPath:
                            new FileInfo(kv.Value.ToString());
                            AppConfig.ProvidersPath = kv.Value.ToString();
                            break;
                        case UpdateFrequency:
                            int val = Convert.ToInt32(kv.Value);
                            AppConfig.UpdateFrequency = val;
                            break;
                        case UpdateTimeOut:
                            int val2 = Convert.ToInt32(kv.Value);
                            AppConfig.UpdateTimeout = val2;
                            break;
                        default:
                            throw new Exception("Setting invalid");                            
                    }
                }

                AppConfig.Save();

                this.Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(this, ex);
            }
        }

        /*private void ProvidersPathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            configsViewModel.ProvidersPath = fbd.SelectedPath;
        }*/

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cancelled = true;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cancelled = false;
            var data = DlgControl.GetOptionsData();
        }


        public bool Retry { get; set; }
    }
}
