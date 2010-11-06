using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaInTheCloud
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = parameter != null && (bool.Parse((string)parameter) == true);

            bool input = (bool)value;
            if (input & !inverted)
                return Visibility.Visible;
            if (!input & inverted)
                return Visibility.Visible;

            return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = parameter != null && ((bool)parameter == true);

            var visibility = (Visibility)value;
            if (visibility == Visibility.Visible & !inverted)
                return true;
            if (visibility == Visibility.Collapsed & inverted)
                return true;
            return false;
        }

    }
}
