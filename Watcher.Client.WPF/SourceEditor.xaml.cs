using Watcher.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Watcher.Client.WPF.ViewModels;
using Innouvous.Utils.MVVM;
using Watcher.Extensions.V2;

namespace Watcher.Client.WPF
{

    //TODO: Convert to MVVM

    /// <summary>
    /// Interaction logic for SourceEditor.xaml
    /// </summary>
    public partial class SourceEditor : Window
    {
        //private AbstractDataStore datastore;
        //private List<AbstractProvider> providers;

        //private Color selectedColor = SourceViewModel.DefaultColor;

        //private SourceViewModel sourceViewModel;

        private readonly SourceEditorViewModel vm;

        public SourceEditor()
            : this(DataManager.Instance().GetProviders(), DataManager.Instance().DataStore, null)
        {}

        public SourceEditor(SourceViewModel svm) : 
            this(DataManager.Instance().GetProviders(), DataManager.Instance().DataStore, svm)
        {

        }
            
        public SourceEditor(List<AbstractProvider> providers, AbstractDataStore dataStore, SourceViewModel svm)
        {
            InitializeComponent();

            vm = new SourceEditorViewModel(this, OptionsGrid, providers, dataStore, svm);
            this.DataContext = vm;

            /*

            CommonLoad(providers, dataStore);

            sourceViewModel = svm;

            LoadFromSource(svm.Data);
            */

        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vm.PickColorCommand.Execute(null);
        }

        public SourceViewModel GetSourceViewModel()
        {
            return vm.GetSourceViewModel();
        }
    }
}
