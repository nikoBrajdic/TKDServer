using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TKD.App.Models;

namespace TKD.App.Controllers
{
    //[NotifyPropertyChanged]
    public class DoubleTextConverter : IValueConverter, INotifyPropertyChanged
    {
        private Score score;
        public Score Score {
            get => score;
            set => SetField(ref score, value);
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void SetField(ref Score score, Score value)
        {
            if (score != value)
            {
                score = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString("0.0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString());
        }
    }
}
