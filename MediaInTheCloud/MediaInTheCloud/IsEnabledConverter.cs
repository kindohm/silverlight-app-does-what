using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaInTheCloud
{
    public class IsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = parameter != null && (bool.Parse((string)parameter) == true);

            bool input = (bool)value;
            if (input & !inverted)
                return true;
            if (!input & inverted)
                return true;

            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = parameter != null && ((bool)parameter == true);

            var isEnabled = (bool)value;
            if (isEnabled & !inverted)
                return true;
            if (isEnabled & inverted)
                return true;
            return false;
        }

    }
}
