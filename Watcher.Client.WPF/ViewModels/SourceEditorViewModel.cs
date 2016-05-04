using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Watcher.Extensions.V2;
using Watcher.Interop;

namespace Watcher.Client.WPF.ViewModels
{
    public class SourceEditorViewModel : ViewModel
    {
        private IDataStore datastore;
        private SourceViewModel originalSource;


        //TODO: Create grid and populate here
        private Grid optionsGrid;

        #region Properties
        private List<IProvider> providers;
        public List<IProvider> Providers
        {
            get
            {
                return providers;
            }
            private set
            {
                providers = value;
                RaisePropertyChanged("Providers");
            }
        }


        //TODO: Add to XAML
        public string IDText { get; private set; }

        //TODO: Add to XAML
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                RaisePropertyChanged("Url");
            }
        }

        private bool disabled;
        public bool Disabled
        {
            get
            {
                return disabled;
            }
            set
            {
                disabled = value;
                RaisePropertyChanged();
            }
        }

        private bool canSelectProvider = true;
        public bool CanSelectProvider
        {
            get
            {
                return canSelectProvider;
            }
            private set
            {
                canSelectProvider = value;
                RaisePropertyChanged("CanSelectProvider");
            }
        }

        public Visibility NameVisibility
        {
            get
            {
                return (selectedProvider != null && selectedProvider.HasUniqueName) ? Visibility.Visible : Visibility.Collapsed; 
            }
        }

        public Visibility URLVisibility
        {
            get
            {
                return selectedProvider != null && selectedProvider.HasUrlField ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private IProvider selectedProvider;
        public IProvider SelectedProvider
        {
            get
            {
                return selectedProvider;
            }
            set
            {
                try
                {
                    
                    //This is to support restoring values from existing, need to rewrite and separate, make clearer?
                    List<IMetaDataObject> meta = originalSource != null ?
                        new List<IMetaDataObject>(originalSource.Data.GetMetaData().Values) :
                        value.GetMetaFields();

                    optionsGrid.Children.Clear();
                    optionsGrid.RowDefinitions.Clear();

                    int rowCounter = 0;

                    Dictionary<string, IMetaDataObject> values = null;

                    if (originalSource != null)
                    {
                        values = originalSource.Data.GetMetaData();
                    }

                    foreach (var m in meta)
                    {
                        //Remove fields belonging to UI
                        if (SourceViewModel.ClientTypes.Contains(m.ID))
                            continue;

                        RowDefinition rd = new RowDefinition() { Height = GridLength.Auto };
                        optionsGrid.RowDefinitions.Add(rd);
                        

                        Label l = new Label();
                        l.Content = m.DisplayName;
                        l.SetValue(Grid.RowProperty, rowCounter);
                        l.SetValue(Grid.ColumnProperty, 0);
                        optionsGrid.Children.Add(l);

                        Control control = null;

                        string val = null;
                        if (values != null && values.ContainsKey(m.ID))
                            val = m.GetValueAsString();

                        switch (m.FieldType)
                        {
                            case MetaDataObjectType.NA:
                                break;
                            case MetaDataObjectType.Selector:
                                var cb = new ComboBox();
                                cb.ItemsSource = m.SelectorValues;
                                cb.Text = val;

                                control = cb;
                                break;
                            case MetaDataObjectType.String:
                                TextBox tb = new TextBox();
                                tb.Text = val;

                                control = tb;
                                break;
                            case MetaDataObjectType.CheckBox:
                                CheckBox check = new CheckBox();
                                check.IsChecked = Convert.ToBoolean(val);
                                control = check;
                                break;
                            default:
                                throw new NotSupportedException(m.FieldType.ToString());
                        }

                        if (control != null)
                        {
                            control.SetValue(Grid.RowProperty, rowCounter);
                            control.SetValue(Grid.ColumnProperty, 1);
                            control.Tag = m.ID;

                            optionsGrid.Children.Add(control);
                        }

                        rowCounter++;
                    }
                    
                    selectedProvider = value;
                    RaisePropertyChanged("SelectedProvider");
                    RaisePropertyChanged("NameVisilibility");
                    RaisePropertyChanged("URLVisilibility");
                }
                catch (Exception ex)
                {
                    MessageBoxFactory.ShowError(ex);
                }
            }
        }


        private Color selectedColor = SourceViewModel.DefaultColor;

        //TODO: Add to XAML, Fill =
        public SolidColorBrush PreviewColor
        {
            get
            {
                return new SolidColorBrush(selectedColor);
            }
        }


        private const string IDVisibilityProperty = "IDVisibility";
        private Window window;

        //TODO: Add to XAML
        public Visibility IDVisibility
        {
            get
            {
                return originalSource == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        /// <summary>
        /// Gets the source view model created/edited by SourceEditor
        /// </summary>
        /// <returns>The a SourceViewModel including the updated Source object</returns>
        public SourceViewModel GetSourceViewModel()
        {
            return originalSource;
        }

        #endregion


        public SourceEditorViewModel(Window window, Grid optionsGrid, List<IProvider> providers, IDataStore dataStore, SourceViewModel svm = null)
        {
            this.window = window;
            this.optionsGrid = optionsGrid;

            this.Providers = providers;
            this.datastore = dataStore;
            
            if (svm != null)
                SetOriginalSource(svm);
        }

        private void SetOriginalSource(SourceViewModel svm)
        {
            originalSource = svm;
            RaisePropertyChanged(IDVisibilityProperty);

            var source = svm.Data;

            IDText = source.ID.Value.ToString();
            Name = source.SourceName;
            Url = SourceViewModel.GetUrl(source);
            Disabled = svm.Disabled;

            CanSelectProvider = false;
            SelectedProvider = providers.First(p => p.ProviderId == source.ProviderID);

            //TODO:ADD
            //TypeComboBox.SelectedItem = providers.First(p => p.ProviderId == source.ProviderID);
            //TypeComboBox.IsEnabled = false;

            SetSelectedColor(SourceViewModel.GetColor(source));
        }

        private void SetSelectedColor(Color color)
        {
            selectedColor = color;

            //Make the SolidColor once?
            RaisePropertyChanged("PreviewColor");

            //ColorPreviewRectangle.Fill = new SolidColorBrush(selectedColor);            
        }



        #region Commands
        

        public ICommand OKCommand
        {
            get
            {
                return new CommandHelper(ProcessParameters);
            }
        }

        private void ProcessParameters()
        {
            try
            {
                if (SelectedProvider == null)
                    throw new Exception("No provider selected");
                
                var meta = SelectedProvider.GetMetaFields();

                foreach (Control c in optionsGrid.Children)
                {
                    if (c.Tag != null)
                    {
                        string id = c.Tag.ToString();
                        string value = null;

                        if (c is TextBox)
                            value = ((TextBox)c).Text;
                        else if (c is ComboBox)
                            value = ((ComboBox)c).Text;
                        else if (c is CheckBox)
                            value = ((CheckBox)c).IsChecked.ToString();
                        else
                        {
                            //Error
                        }

                        
                        var item = meta.FirstOrDefault(x => x.ID == id);

                        if (item != null) //TODO: Disabled hack
                        item.SetValue(value);
                    }
                }

                var newSource = selectedProvider.CreateNewSource(
                    selectedProvider.HasUniqueName ? Name : null //Probably just use name
                    , null, meta);

                if (selectedProvider.HasUrlField)
                {
                    newSource.SetMetaData(new MetaDataObject(SourceViewModel.URL, SourceViewModel.URL, url));
                }

                newSource.SetMetaData(new MetaDataObject(SourceViewModel.UPDATES_COLOR, SourceViewModel.UPDATES_COLOR, SourceViewModel.SerializeColor(selectedColor)));
                newSource.SetMetaData(new MetaDataObject(SourceViewModel.DISABLED, SourceViewModel.DISABLED, disabled));
                
                if (originalSource != null)
                {
                    newSource.SetID(originalSource.Data.ID.Value);

                    datastore.UpdateSource(newSource);

                    originalSource.SetSource(newSource);
                }
                else
                {
                    datastore.AddSource(newSource);

                    originalSource = new SourceViewModel(newSource);
                }
                
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBoxFactory.ShowError(ex);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() =>
                {
                    originalSource = null;
                    window.Close();
                });
            }
        }


        public ICommand PickColorCommand
        {
            get
            {
                return new CommandHelper(PickColor);
            }
        }
        private void PickColor()
        {
            ColorPicker dlg = new ColorPicker();
            
            dlg.ShowDialog();

            if (dlg.SelectedColor != null)
            {
                //Get SelectedColor
                Color c = dlg.SelectedColor.Value;
                //selectedColor = c;
                SetSelectedColor(c);
                //ColorPreviewRectangle.Fill = new SolidColorBrush(c);
            }
        }


        #endregion


    }
}
