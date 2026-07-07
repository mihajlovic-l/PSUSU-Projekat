using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataConcentrator;

namespace ScadaWPF.Converters
{
    // ─── AlarmStateToColorConverter ───────────────────────────────────────────
    // Used by the alarm list to colour rows:
    //   Active       → red   (user hasn't acknowledged yet)
    //   Acknowledged → yellow (user saw it but value still out of range)
    //   Inactive     → transparent (normal)
    public class AlarmStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            if (value is AlarmState state)
            {
                switch (state)
                {
                    case AlarmState.Active:         return new SolidColorBrush(Color.FromRgb(0x5C, 0x2B, 0x2B)); // dark red   (#5C2B2B)
                    case AlarmState.Acknowledged:   return new SolidColorBrush(Color.FromRgb(0x5C, 0x4A, 0x1E)); // dark amber (#5C4A1E)
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
