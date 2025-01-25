using System;
using System.Windows.Data;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace AccraRoadAttendance.Converters
{
    public class EllipsisVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is List<int> pageNumbers)
            {
                // Check if the list contains an ellipsis (-1)
                return pageNumbers.Contains(-1) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}