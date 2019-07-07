using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for PoomsaeChoice.xaml
    /// </summary>
    public partial class PoomsaeChoice : Window
    {
        public PoomsaeChoice()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = ((Button)sender).Content.ToString() == "1" ? true : false;
            Close();
        }
    }
}
