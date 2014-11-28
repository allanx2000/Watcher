using Innouvous.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Watcher.Client.WPF
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class SimpleDialogWindow : DialogWindow
    {
        private DialogControlOptions options;

        public SimpleDialogWindow(DialogControlOptions options)
        {
            InitializeComponent();

            this.Title = options.Title;
            this.options = options;

            options.CloseAction += (sender, e) => {
                this.CanClose();
                this.Close(); 
            };

            DialogControl.SetupControl(options);
        }
    }
}
