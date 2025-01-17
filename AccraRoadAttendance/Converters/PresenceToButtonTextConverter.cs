using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AccraRoadAttendance.Converters
{
    public class PresenceToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Check Out" : "Check In";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}