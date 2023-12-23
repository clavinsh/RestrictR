using Microsoft.UI.Xaml.Data;
using System;

namespace RestrictR
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString("hh\\:mm");
            }

            return TimeSpan.Zero.ToString("hh\\:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (TimeSpan.TryParse(value.ToString(), out TimeSpan result))
            {
                return result;
            }

            return TimeSpan.Zero;
        }
    }
}
