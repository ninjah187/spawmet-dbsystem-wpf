using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SpawmetDatabase;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Converters
{
    public class OriginToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var origin = (Origin)value;
            switch (origin)
            {
                case Origin.Outside:
                    return "Zewnątrz";

                case Origin.Production:
                    return "Produkcja";

                default:
                    throw new InvalidOperationException("There's no such Origin enum value.");
            }
            //var origin = (Origin) value;
            //return origin.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var origin = (string) value;
            switch (origin)
            {
                case "Zewnątrz":
                    return Origin.Outside;

                case "Produkcja":
                    return Origin.Production;

                default:
                    throw new InvalidOperationException("There's no such Origin enum value.");
            }
        }
    }
}
