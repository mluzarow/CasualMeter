using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CasualMeter.Tracker;

namespace CasualMeter.Common.Converters
{
    public class DoubleToPercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(value is double))
                throw new ArgumentException($"Invalid arguments passed to {nameof(DoubleToPercentStringConverter)}.");

            var helper = FormatHelpers.Pretty;
            return helper.FormatPercent((double?) value ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
