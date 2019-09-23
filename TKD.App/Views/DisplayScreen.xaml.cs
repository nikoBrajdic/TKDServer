using System.Windows;
using System.Windows.Forms;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for DisplayScreen.xaml
    /// </summary>
    public partial class Audience : Window
    {
        public Audience()
        {
            InitializeComponent();
        }

        private void DisplayScreenWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Screen.AllScreens.Length > 1)
            {
                Left = Screen.AllScreens[1].Bounds.Left;
                Top = Screen.AllScreens[1].Bounds.Top;
            }

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }
    }
}
