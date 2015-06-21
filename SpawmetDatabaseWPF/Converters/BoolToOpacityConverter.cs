using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SpawmetDatabaseWPF.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        private readonly double _trueOpacity = 1.0d;
        private readonly double _falseOpacity = 0.5d;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = (bool) value;

            if (boolean == true)
            {
                return _trueOpacity;
            }
            else
            {
                return _falseOpacity;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var opacity = (double) value;

            if (opacity == _trueOpacity)
            {
                return _trueOpacity;
            }
            else
            {
                return _falseOpacity;
            }
        }
    }
}
