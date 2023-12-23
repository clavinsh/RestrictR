using DataPacketLibrary.Models;
using Microsoft.UI.Xaml.Data;
using System;

namespace RestrictR.Converters
{
    internal class StartDurationToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Event obj)
            {
                return $"{obj.Start} - {obj.Start.Add(obj.Duration).TimeOfDay}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
