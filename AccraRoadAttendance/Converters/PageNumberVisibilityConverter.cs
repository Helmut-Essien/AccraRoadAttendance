using System;
using System.Windows.Data;
using System.Windows;

namespace AccraRoadAttendance.Converters
{
    public class PageNumberVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int pageNumber)
            {
                // Show all page numbers except the ellipsis
                if (pageNumber != -1)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}