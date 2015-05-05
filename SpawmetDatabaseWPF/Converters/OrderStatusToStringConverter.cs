using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (OrderStatus) value;
            switch (status)
            {
                case OrderStatus.Done:
                    return "Zakończone";

                case OrderStatus.InProgress:
                    return "W toku";

                case OrderStatus.New:
                    return "Nowe";

                default:
                    throw new InvalidOperationException("There's no such OrderStatus.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (string) value;
            switch (status)
            {
                case "Zakończone":
                    return OrderStatus.Done;

                case "W toku":
                    return OrderStatus.InProgress;

                case "Nowe":
                    return OrderStatus.New;

                default:
                    throw new InvalidOperationException("There's no such OrderStatus.");
            }
        }
    }
}
