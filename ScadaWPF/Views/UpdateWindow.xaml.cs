using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaWPF.Views
{
    // UpdateWindow: koristi se za izmenu taga ili alarma.
    // Konstruktor prima Tag ili Alarm i popunjava odgovarajuća polja.
    public partial class UpdateWindow : Window
    {
        private readonly Tag   _tag;    // postavljeno pri izmeni taga
        private readonly Alarm _alarm;  // postavljeno pri izmeni alarma

        // Konstruktor za izmenu taga
        public UpdateWindow(Tag tag)
        {
            InitializeComponent();
            _tag = tag;
            PopulateForTag();
        }

        // Konstruktor za izmenu alarma
        public UpdateWindow(Alarm alarm)
        {
            InitializeComponent();
            _alarm = alarm;
            PopulateForAlarm();
        }

        // Popuni polja za izmenu taga
        private void PopulateForTag()
        {
            TxtHeader.Text = $"Update {_tag.TagType} Tag — {_tag.Name}";

            // Ime je samo za čitanje; ne može se menjati zbog primarnog ključa
            PanelTagShared.Visibility = Visibility.Visible;
            TxtName.Text        = _tag.Name;
            TxtDescription.Text = _tag.Description;

            if (_tag is AnalogInput ai)
            {
                PanelInput.Visibility  = Visibility.Visible;
                PanelAnalog.Visibility = Visibility.Visible;
                PanelAiOnly.Visibility = Visibility.Visible;

                TxtScanTime.Text       = ai.ScanTime.ToString();
                ChkScanEnabled.IsChecked = ai.ScanEnabled;
                TxtLow.Text            = ai.LowLimit.ToString(CultureInfo.InvariantCulture);
                TxtHigh.Text           = ai.HighLimit.ToString(CultureInfo.InvariantCulture);
                TxtUnits.Text          = ai.Units;
                TxtDeadband.Text       = ai.Deadband.ToString(CultureInfo.InvariantCulture);
                TxtHysteresis.Text     = ai.Hysteresis.ToString(CultureInfo.InvariantCulture);
            }
            else if (_tag is AnalogOutput ao)
            {
                PanelAnalog.Visibility = Visibility.Visible;
                PanelOutput.Visibility = Visibility.Visible;

                TxtLow.Text          = ao.LowLimit.ToString(CultureInfo.InvariantCulture);
                TxtHigh.Text         = ao.HighLimit.ToString(CultureInfo.InvariantCulture);
                TxtUnits.Text        = ao.Units;
                TxtInitialValue.Text = ao.InitialValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (_tag is DigitalInput di)
            {
                PanelInput.Visibility = Visibility.Visible;

                TxtScanTime.Text         = di.ScanTime.ToString();
                ChkScanEnabled.IsChecked = di.ScanEnabled;
            }
            else if (_tag is DigitalOutput doo)
            {
                PanelOutput.Visibility = Visibility.Visible;
                // Prikaži trenutnu vrednost kao 0/1
                TxtInitialValue.Text = doo.InitialValue ? "1" : "0";
            }
        }

        // Popuni polja za izmenu alarma
        private void PopulateForAlarm()
        {
            TxtHeader.Text = $"Update Alarm — Tag: {_alarm.TagName}";

            PanelAlarm.Visibility = Visibility.Visible;

            TxtAlarmTag.Text    = _alarm.TagName;
            TxtAlarmLimit.Text  = _alarm.Limit.ToString(CultureInfo.InvariantCulture);
            CboAlarmDir.SelectedIndex = _alarm.IsHighAlarm ? 0 : 1;
            TxtAlarmMsg.Text    = _alarm.Message;
        }

        // Sačuvaj promene
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_alarm != null)
                    SaveAlarm();
                else
                    SaveTag();
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"UpdateWindow save error: {ex.Message}");
                MessageBox.Show(ex.Message, "Validation error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Logika za čuvanje izmena taga
        private void SaveTag()
        {
            if (_tag is AnalogInput ai)
            {
                ai.Description = TxtDescription.Text.Trim();
                ai.ScanTime    = RequireInt(TxtScanTime, "Scan Time");
                ai.ScanEnabled = ChkScanEnabled.IsChecked == true;
                ai.LowLimit    = RequireDouble(TxtLow, "Low Limit");
                ai.HighLimit   = RequireDouble(TxtHigh, "High Limit");
                ai.Units       = TxtUnits.Text.Trim();
                ai.Deadband    = RequireDouble(TxtDeadband, "Deadband");
                ai.Hysteresis  = RequireDouble(TxtHysteresis, "Hysteresis");

                if (ai.HighLimit <= ai.LowLimit)
                    throw new Exception("High Limit must be greater than Low Limit.");

                TagManager.Instance.UpdateTag(ai);
            }
            else if (_tag is AnalogOutput ao)
            {
                ao.Description  = TxtDescription.Text.Trim();
                ao.LowLimit     = RequireDouble(TxtLow, "Low Limit");
                ao.HighLimit    = RequireDouble(TxtHigh, "High Limit");
                ao.Units        = TxtUnits.Text.Trim();
                ao.InitialValue = RequireDouble(TxtInitialValue, "Initial Value");

                if (ao.HighLimit <= ao.LowLimit)
                    throw new Exception("High Limit must be greater than Low Limit.");

                TagManager.Instance.UpdateTag(ao);
            }
            else if (_tag is DigitalInput di)
            {
                di.Description = TxtDescription.Text.Trim();
                di.ScanTime    = RequireInt(TxtScanTime, "Scan Time");
                di.ScanEnabled = ChkScanEnabled.IsChecked == true;

                TagManager.Instance.UpdateTag(di);
            }
            else if (_tag is DigitalOutput doo)
            {
                doo.Description  = TxtDescription.Text.Trim();
                doo.InitialValue = RequireDigitalValue(TxtInitialValue, "Initial Value");

                TagManager.Instance.UpdateTag(doo);
            }
        }

        // Logika za čuvanje alarma
        private void SaveAlarm()
        {
            _alarm.Limit       = RequireDouble(TxtAlarmLimit, "Limit Value");
            _alarm.IsHighAlarm = CboAlarmDir.SelectedIndex == 0;
            _alarm.Message     = TxtAlarmMsg.Text.Trim();

            TagManager.Instance.UpdateAlarm(_alarm);
        }

        // ── Validation helpers ────────────────────────────────────────────────

        private double RequireDouble(TextBox tb, string fieldName)
        {
            if (!double.TryParse(tb.Text.Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double v))
                throw new Exception($"{fieldName} must be a valid number (use '.' as decimal separator).");
            return v;
        }

        private int RequireInt(TextBox tb, string fieldName)
        {
            if (!int.TryParse(tb.Text.Trim(), out int v) || v <= 0)
                throw new Exception($"{fieldName} must be a positive integer.");
            return v;
        }

        private bool RequireDigitalValue(TextBox tb, string fieldName)
        {
            string v = tb.Text.Trim();
            if (v == "0") return false;
            if (v == "1") return true;
            throw new Exception($"{fieldName} must be 0 or 1.");
        }
    }
}
