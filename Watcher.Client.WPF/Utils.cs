using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Watcher.Client.WPF
{
    class Utils
    {


        public static void ShowErrorMessage(Window owner, Exception e)
        {
            MessageBox.Show(owner, e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
