using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TKD.App.Controllers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class RefereeVisibility : IValueConverter
    {
        public int refNo { get => MainWindow.Settings.RefNo; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)parameter < refNo ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
