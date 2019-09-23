using System.Windows;
using System.Windows.Controls;

namespace TKD.App.Views
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow() => InitializeComponent();
        private void ClearText(object sender, RoutedEventArgs e) => LogBox.Text = "";
        public void Log(string text)
        {
            LogBox.Text += text;
            LogBox.ScrollToEnd();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Hide();
    }
}
