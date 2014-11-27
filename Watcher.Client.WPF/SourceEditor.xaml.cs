using Watcher.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Watcher.Client.WPF.ViewModels;
using Watcher.Core.Internal;

namespace Watcher.Client.WPF
{

    /// <summary>
    /// Interaction logic for SourceEditor.xaml
    /// </summary>
    public partial class SourceEditor : Window
    {
        private class MetaData : ObservableClass
        {
            public string Name { get; private set; }

            private string val;
            public string Value
            {
                get
                {
                    return val;
                }
                set
                {
                    val = value;
                    RaiseEvent("Value");
                }
            }

            public MetaData(string name, string value = null)
            {
                Name = name;
                Value = value;
            }
        }

        //private AbstractSource currentSource;

        private AbstractDataStore dataStore;
        private List<AbstractProvider> providers;

        private Color selectedColor = AbstractSource.DefaultColor;

        private SourceViewModel sourceViewModel;

        #region Constructors

        public SourceEditor()
            : this(DataManager.Instance().GetProviders(), DataManager.Instance().DataStore)
        {}

        public SourceEditor(SourceViewModel svm) : 
            this(DataManager.Instance().GetProviders(), DataManager.Instance().DataStore, svm)
        {

        }
            
        public SourceEditor(List<AbstractProvider> providers, AbstractDataStore dataStore, SourceViewModel svm)
        {
            CommonLoad(providers, dataStore);

            sourceViewModel = svm;

            LoadFromSource(svm.Data);

        }

        public SourceEditor(List<AbstractProvider> providers, AbstractDataStore dataStore)
        {

            CommonLoad(providers, dataStore);

            IDPanel.Visibility = System.Windows.Visibility.Collapsed;

            ColorPreviewRectangle.Fill = new SolidColorBrush(selectedColor);
        }


        private void CommonLoad(List<AbstractProvider> providers, AbstractDataStore dataStore)
        {
            this.providers = providers;

            this.dataStore = dataStore;

            InitializeComponent();

            TypeComboBox.ItemsSource = providers;
            TypeComboBox.DisplayMemberPath = "ProviderId";

        }


        private void LoadFromSource(AbstractSource source)
        {
            //currentSource = source;

            IDField.Content = source.ID.Value;
            NameTextBox.Text = source.SourceName;
            URLTextBox.Text = source.Url;

            TypeComboBox.SelectedItem = providers.First(p => p.ProviderId == source.ProviderID);
            TypeComboBox.IsEnabled = false;

            IDPanel.Visibility = System.Windows.Visibility.Visible;


            selectedColor = source.UpdatesColor;

            ColorPreviewRectangle.Fill = new SolidColorBrush(selectedColor);


        }

        #endregion

        /// <summary>
        /// Gets the source view model created/edited by SourceEditor
        /// </summary>
        /// <returns>The a SourceViewModel including the updated Source object</returns>
        public SourceViewModel GetSourceViewModel()
        {
            return sourceViewModel;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TypeComboBox.SelectedItem == null)
                    throw new Exception("No provider selected");

                var selectedProvider = (AbstractProvider)TypeComboBox.SelectedItem;
                var so = selectedProvider.GetSourceOptions();

                var meta = new Dictionary<string, string>();
                foreach (MetaData md in ProviderOptionsListBox.ItemsSource)
                {
                    if (!String.IsNullOrEmpty(md.Value))
                        meta.Add(md.Name, md.Value);
                }


                var newSource = selectedProvider
                        .CreateNewSource(
                            so.HasUniqueName ? NameTextBox.Text : null,
                            so.HasURLField ? URLTextBox.Text : null,
                            meta);

                newSource.SetUpdatesColor(selectedColor);

                if (sourceViewModel != null)
                {
                    newSource.SetID(sourceViewModel.Data.ID.Value);

                    dataStore.UpdateSource(newSource);

                    sourceViewModel.SetSource(newSource);
                }
                else
                {
                    dataStore.AddSource(newSource);

                    sourceViewModel = new SourceViewModel(newSource);
                }



                this.Close();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(this, ex);
                //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            sourceViewModel = null;

            this.Close();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (e.AddedItems.Count != 0)
                    ReloadFields((AbstractProvider) e.AddedItems[0]);
            }
            catch (Exception ex)
            {
                e.Handled = true;
                Utils.ShowErrorMessage(this,ex);
            }
        }

        private void ReloadFields(AbstractProvider provider)
        {
            

            SourceOptions options = provider.GetSourceOptions();

            NamePanel.Visibility = options.HasUniqueName ? Visibility.Visible : Visibility.Collapsed;

            URLPanel.Visibility = options.HasURLField ? Visibility.Visible : Visibility.Collapsed;

            List<MetaData> metadata = new List<MetaData>();

            foreach (var o in provider.GetMetaFields())
            {
                string value = sourceViewModel != null ?
                    sourceViewModel.Data.GetMetaDataValue(o)
                    : null;

                metadata.Add(new MetaData(o, value));
            }

            ProviderOptionsListBox.ItemsSource = metadata;
        }

        private void ColorPreviewRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorPicker dlg = new ColorPicker();

            dlg.ShowDialog();

            if (dlg.SelectedColor != null)
            {
                //Get SelectedColor
                Color c = dlg.SelectedColor.Value;
                selectedColor = c;
                ColorPreviewRectangle.Fill = new SolidColorBrush(c);
            }
        }

    }
}
