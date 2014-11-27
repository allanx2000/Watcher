using System.Windows;
using System.Windows.Media;

namespace Watcher.Client.WPF
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public Color? SelectedColor {get; private set;}



        public ColorPicker()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = ColorCanvas.SelectedColor;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = null;

            Close();
        }
    }
}
