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
        private AbstractDataStore datastore;
        private List<AbstractProvider> providers;

        private Color selectedColor = SourceViewModel.DefaultColor;

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

            this.datastore = dataStore;

            InitializeComponent();

            TypeComboBox.ItemsSource = providers;
            TypeComboBox.DisplayMemberPath = "ProviderId";

        }


        private void LoadFromSource(AbstractSource source)
        {
            
            IDField.Content = source.ID.Value;
            NameTextBox.Text = source.SourceName;
            URLTextBox.Text = SourceViewModel.GetUrl(source);

            TypeComboBox.SelectedItem = providers.First(p => p.ProviderId == source.ProviderID);
            TypeComboBox.IsEnabled = false;

            IDPanel.Visibility = System.Windows.Visibility.Visible;

            selectedColor = SourceViewModel.GetColor(source);

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

                var meta = selectedProvider.GetMetaFields();

                foreach (Control c in OptionsGrid.Children)
                {
                    if (c.Tag != null)
                    {
                        string id = c.Tag.ToString();
                        string value = null;

                        if (c is TextBox)
                            value = ((TextBox)c).Text;
                        else if (c is ComboBox)
                            value = ((ComboBox)c).Text;
                        else
                        {
                            //Error
                        }

                        var item = MetaDataObject.FindIn(meta, id);
                        item.SetValue(value);
                    }
                }

                var newSource = selectedProvider.CreateNewSource(
                    selectedProvider.HasUniqueName ? NameTextBox.Text : null
                    , null, meta);
                /*
                var newSource = selectedProvider
                        .CreateNewSource(
                            selectedProvider.HasUniqueName ? NameTextBox.Text : null,
                            selectedProvider.HasUrlField ? URLTextBox.Text : null,
                            meta);
                            */

                //TODO: Should use SVM instead of Abstract?
                if (selectedProvider.HasUrlField)
                    newSource.SetMetaDataValue(SourceViewModel.URL, URLTextBox.Text);

                newSource.SetMetaDataValue(SourceViewModel.UPDATES_COLOR, SourceViewModel.SerializeColor(selectedColor));

                if (sourceViewModel != null)
                {
                    newSource.SetID(sourceViewModel.Data.ID.Value);

                    datastore.UpdateSource(newSource);

                    sourceViewModel.SetSource(newSource);
                }
                else
                {
                    datastore.AddSource(newSource);

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

        //TODO: Set To V2
        private void ReloadFields(AbstractProvider provider)
        {
            NamePanel.Visibility = provider.HasUniqueName ? Visibility.Visible : Visibility.Collapsed;

            URLPanel.Visibility = provider.HasUrlField ? Visibility.Visible : Visibility.Collapsed;

            List<MetaDataObject> meta = sourceViewModel != null ? new List<MetaDataObject>(sourceViewModel.Data.GetMetaData().Values) :
                provider.GetMetaFields();

            OptionsGrid.Children.Clear();
            OptionsGrid.RowDefinitions.Clear();

            int rowCounter = 0;

            Dictionary<string,string> values = null;

            if (sourceViewModel != null)
            {
                values = MetaDataObject.ToDictionary(sourceViewModel.Data.GetMetaData());
            }

            foreach (var m in meta)
            {
                RowDefinition rd = new RowDefinition() { Height = GridLength.Auto };
                OptionsGrid.RowDefinitions.Add(rd);

                Label l = new Label();
                l.Content = m.DisplayName;
                l.SetValue(Grid.RowProperty, rowCounter);
                l.SetValue(Grid.ColumnProperty, 0);
                OptionsGrid.Children.Add(l);

                Control control = null;

                string value = null;
                if (values != null && values.ContainsKey(m.ID))
                    value = m.GetValueAsString();

                switch (m.FieldType)
                {
                    case MetaDataObject.Type.NA:
                        break;
                    case MetaDataObject.Type.Selector:
                        var cb = new ComboBox();
                        cb.ItemsSource = m.SelectorValues;
                        cb.Text = value;
                        
                        control = cb;
                        break;
                    case MetaDataObject.Type.String:
                        TextBox tb = new TextBox();
                        tb.Text = value;
                        
                        control = tb;
                        break;
                }

                if (control != null)
                {
                    control.SetValue(Grid.RowProperty, rowCounter);
                    control.SetValue(Grid.ColumnProperty, 1);
                    control.Tag = m.ID;

                    OptionsGrid.Children.Add(control);
                }

                rowCounter++;
            }
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
