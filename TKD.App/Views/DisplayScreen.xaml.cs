using System.Windows;
using System.Windows.Forms;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for DisplayScreen.xaml
    /// </summary>
    public partial class DisplayScreen : Window
    {
        public DisplayScreen()
        {
            InitializeComponent();
        }

        private void DisplayScreenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Screen.AllScreens[1].Bounds.Left;
            Top = Screen.AllScreens[1].Bounds.Top;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }
    }
}
