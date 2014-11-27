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

        private readonly RunConfigsViewModel configsViewModel = new RunConfigsViewModel();
    
        public bool Cancelled
        { get; set; }

        public bool IsFirstRun
        {
            get
            {
                return String.IsNullOrEmpty(configsViewModel.DataStorePath)
                    || String.IsNullOrEmpty(configsViewModel.ProvidersPath);
            }
        }

        public RunConfigsWindow()
        {
            InitializeComponent();

            this.DataContext = configsViewModel;
        }

        private void PathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog { AddExtension = true, CheckPathExists = true, DefaultExt = ".sqlite" };

            var sfdResult = sfd.ShowDialog();

            if (sfdResult == true)
                configsViewModel.DataStorePath = sfd.FileName;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                configsViewModel.SaveConfigs();

                this.Close();
            }
            catch (Exception ex)
            {

                Utils.ShowErrorMessage(this, ex);
            }
        }

        private void ProvidersPathSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            configsViewModel.ProvidersPath = fbd.SelectedPath;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
