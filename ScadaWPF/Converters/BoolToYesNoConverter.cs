using System;
using System.Globalization;
using System.Windows.Data;

namespace ScadaWPF.Converters
{
    // Converts true/false to "Yes"/"No" for display in the tag grid
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
            => (value is bool b && b) ? "Yes" : "No";

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
