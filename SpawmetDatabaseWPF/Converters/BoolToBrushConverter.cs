using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SpawmetDatabaseWPF.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        private readonly SolidColorBrush greenBrush = new SolidColorBrush(Color.FromRgb(0, 200, 0));
        private readonly SolidColorBrush redBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = (bool) value;

            if (boolean == true)
            {
                return greenBrush;
            }
            else
            {
                return redBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = (Brush) value;

            if (brush.Equals(greenBrush))
            //if (brush == greenBrush)
            {
                return true;
            }

            if (brush.Equals(redBrush))
            //if (brush == redBrush)
            {
                return false;
            }

            throw new InvalidOperationException();
        }
    }
}
