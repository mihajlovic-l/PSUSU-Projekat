using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaWPF.Converters
{
    // Pretvara stanje alarma u boju reda u listi alarma.
    // Active -> crvena, Acknowledged -> žuta, Inactive -> prozirno
    public class AlarmStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is AlarmState state)
            {
                switch (state)
                {
                    case AlarmState.Active:         return new SolidColorBrush(Color.FromRgb(0x5C, 0x2B, 0x2B)); // tamno crvena (#5C2B2B)
                    case AlarmState.Acknowledged:   return new SolidColorBrush(Color.FromRgb(0x5C, 0x4A, 0x1E)); // tamno žuta (#5C4A1E)
                    default:                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
