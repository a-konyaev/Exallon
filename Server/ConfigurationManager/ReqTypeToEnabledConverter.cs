using System;
using System.Globalization;
using System.Windows.Data;
using Enterra.V8x1C.DOM;

namespace Exallon.ConfigurationManager
{
    [ValueConversion(typeof(TypeEnum), typeof(bool))]
    public class ReqTypeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ReqTypeToEnabled((TypeEnum)value);
        }

        public static bool ReqTypeToEnabled(TypeEnum type)
        {
            return type != TypeEnum.Unknown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
