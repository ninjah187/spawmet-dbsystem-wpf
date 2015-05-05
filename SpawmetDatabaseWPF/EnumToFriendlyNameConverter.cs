using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using SpawmetDatabase;

namespace SpawmetDatabaseWPF
{
    [ValueConversion(typeof(object), typeof(String))]
    public class EnumToFriendlyNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                if (fieldInfo != null)
                {
                    var attributes =
                        (LocalizedDescriptionAttribute[])
                            fieldInfo.GetCustomAttributes(typeof(LocalizedDescriptionAttribute), false);

                    if (attributes.Length > 0
                        && String.IsNullOrEmpty(attributes[0].Description) == false)
                    {
                        return attributes[0].Description;
                    }
                    else
                    {
                        return value.ToString();
                    }
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object paremeter, CultureInfo culture)
        {
            throw new InvalidOperationException("Can't convert back.");
        }
    }
}
