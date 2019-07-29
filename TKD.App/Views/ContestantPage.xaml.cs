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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TKD.App.Controllers;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for ContestantPage.xaml
    /// </summary>
    public partial class ContestantPage : Page
    {
        public ContestantPage()
        {
            InitializeComponent();
            DataContext = new DoubleTextConverter();
        }
    }
}
