using System;
using System.Globalization;
using System.Windows.Data;

namespace TKD.App.Controllers
{
    public class ShowHideColumn : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value != null ? ((bool)value ? "1*" : "0") : "1*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
