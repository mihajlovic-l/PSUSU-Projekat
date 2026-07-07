using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaWPF.Views
{
    public partial class DetailsWindow : Window
    {
        private readonly Tag _tag;

        public DetailsWindow(Tag tag)
        {
            InitializeComponent();
            _tag = tag;
            Populate();
        }

        // Popuni polja u prozoru prema tipu taga
        private void Populate()
        {
            // Zajednička polja za sve tipove tagova
            ValName.Text        = _tag.Name;
            ValType.Text        = _tag.TagType;
            ValDescription.Text = string.IsNullOrEmpty(_tag.Description) ? "—" : _tag.Description;
            ValAddress.Text     = _tag.IOAddress;

            // Sakrij opcione sekcije, prikaži po tipu
            PanelScan.Visibility   = Visibility.Collapsed;
            PanelAnalog.Visibility = Visibility.Collapsed;
            PanelAiOnly.Visibility = Visibility.Collapsed;
            PanelAlarms.Visibility = Visibility.Collapsed;

            if (_tag is AnalogInput ai)
            {
                TxtTitle.Text    = $"Analog Input — {ai.Name}";

                PanelScan.Visibility   = Visibility.Visible;
                PanelAnalog.Visibility = Visibility.Visible;
                PanelAiOnly.Visibility = Visibility.Visible;
                PanelAlarms.Visibility = Visibility.Visible;

                ValScanTime.Text    = ai.ScanTime.ToString();
                ValScanEnabled.Text = ai.ScanEnabled ? "Yes" : "No";
                ValLow.Text         = ai.LowLimit.ToString("G");
                ValHigh.Text        = ai.HighLimit.ToString("G");
                ValUnits.Text       = string.IsNullOrEmpty(ai.Units) ? "—" : ai.Units;
                ValDeadband.Text    = ai.Deadband.ToString("G");
                ValHysteresis.Text  = ai.Hysteresis.ToString("G");

                // Učitaj samo alarm-e vezane za ovaj AI tag
                AlarmList.ItemsSource = TagManager.Instance.Alarms
                    .Where(a => a.TagName == ai.Name)
                    .ToList();
            }
            else if (_tag is AnalogOutput ao)
            {
                TxtTitle.Text    = $"Analog Output — {ao.Name}";

                PanelAnalog.Visibility = Visibility.Visible;

                ValLow.Text          = ao.LowLimit.ToString("G");
                ValHigh.Text         = ao.HighLimit.ToString("G");
                ValUnits.Text        = string.IsNullOrEmpty(ao.Units) ? "—" : ao.Units;
            }
            else if (_tag is DigitalInput di)
            {
                TxtTitle.Text    = $"Digital Input — {di.Name}";

                PanelScan.Visibility = Visibility.Visible;

                ValScanTime.Text     = di.ScanTime.ToString();
                ValScanEnabled.Text  = di.ScanEnabled ? "Yes" : "No";
            }
            else if (_tag is DigitalOutput doo)
            {
                TxtTitle.Text    = $"Digital Output — {doo.Name}";

            }
        }

        // Potvrdi alarm (Acknowledge)
        private void BtnAck_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmList.SelectedItem is Alarm alarm)
            {
                TagManager.Instance.AcknowledgeAlarm(alarm.Id);
                RefreshAlarmList();
            }
            else
                MessageBox.Show("Select an alarm to acknowledge.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Ažuriraj izabrani alarm
        private void BtnUpdateAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmList.SelectedItem is Alarm alarm)
            {
                var win = new UpdateWindow(alarm) { Owner = this };
                win.ShowDialog();
                RefreshAlarmList();  // odmah prikaži eventualne izmene
            }
            else
                MessageBox.Show("Select an alarm to update.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Obriši izabrani alarm
        private void BtnDeleteAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmList.SelectedItem is Alarm alarm)
            {
                var result = MessageBox.Show(
                    $"Delete alarm '{alarm.Message}' on tag '{alarm.TagName}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    TagManager.Instance.RemoveAlarm(alarm.Id);
                    RefreshAlarmList();
                }
            }
            else
                MessageBox.Show("Select an alarm to delete.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Osveži listu alarma nakon izmene
        private void RefreshAlarmList()
        {
            AlarmList.ItemsSource = TagManager.Instance.Alarms
                .Where(a => a.TagName == _tag.Name)
                .ToList();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        // Klik na prazno mesto u listi alarma -> poništi selekciju
        private void AlarmList_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hit = System.Windows.Media.VisualTreeHelper.HitTest(AlarmList, e.GetPosition(AlarmList));
            if (hit == null) return;

            var item = FindAncestor<System.Windows.Controls.ListViewItem>(
                hit.VisualHit as System.Windows.DependencyObject);

            if (item == null)
                AlarmList.SelectedItem = null;
        }

        private static T FindAncestor<T>(System.Windows.DependencyObject current)
            where T : System.Windows.DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
