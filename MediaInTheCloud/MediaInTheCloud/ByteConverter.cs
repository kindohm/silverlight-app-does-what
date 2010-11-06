// this code based from http://silverlightfileupld.codeplex.com/

using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaInTheCloud
{
    public class ByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string size = "0 KB";

            if (value != null)
            {
                long byteCount = (long)value;

                if (byteCount >= 1073741824)
                    size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
                else if (byteCount >= 1048576)
                    size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
                else if (byteCount >= 1024)
                    size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
                else if (byteCount > 0 && byteCount < 1024)
                    size = "1 KB";    //Bytes are unimportant ;)            

            }

            return size;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
