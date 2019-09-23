using System;
using System.Globalization;
using System.Windows.Data;

namespace TKD.App.Controllers
{
    public class ScoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Console.WriteLine($"converting {value}, type: {value.GetType()}");
                return ((double)value).ToString((string)parameter == "mean" ? "0.00" : "0.0");
            }
            return "0.0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                Console.WriteLine($"converting back {value}, type: {value.GetType()}");
                return double.Parse(value.ToString());
            }
            return 0.0;
        }
    }
}
