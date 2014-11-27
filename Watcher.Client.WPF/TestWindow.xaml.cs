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
    public partial class TestWindow : DialogWindow
    {
        public TestWindow()
        {
            InitializeComponent();

            DialogControlOptions opts;

            /*opts = DialogControlOptions.SetTextBoxMessageOptions(
                "Test", "this is a test", false,
                (sender, args) => 
                {
                    this.CanClose();
                    this.Close();
                });
            */

            List<string> fields = new List<string>()
            {
                "Field1","Field2","Field3"
            };

            opts = DialogControlOptions.SetDataInputOptions(
                "Test", "this is a test", fields,
                (sender, args) =>
                {
                    this.CanClose();
                    this.Close();
                });

            this.Title = opts.Title;

            DialogControl.SetupControl(opts);
        }
    }
}
