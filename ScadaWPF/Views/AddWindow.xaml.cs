using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;

namespace ScadaWPF.Views
{
    public partial class AddWindow : Window
    {
        // ── Valid address pools per tag type ──────────────────────────────────
        // Must match exactly what PLCSimulatorManager has in its dictionary.
        private static readonly List<string> AI_ADDRESSES =
            new List<string> { "ADDR001", "ADDR002", "ADDR003", "ADDR004" };
        private static readonly List<string> AO_ADDRESSES =
            new List<string> { "ADDR005", "ADDR006", "ADDR007", "ADDR008" };
        private static readonly List<string> DI_ADDRESSES =
            new List<string> { "ADDR009", "ADDR011", "ADDR012", "ADDR013" };
        private static readonly List<string> DO_ADDRESSES =
            new List<string> { "ADDR010", "ADDR014", "ADDR015", "ADDR016" };

        public AddWindow()
        {
            InitializeComponent();
            RefreshAlarmTagList();
            ShowPanelsForType("AI");
        }

        // ── Type selector changed ─────────────────────────────────────────────
        private void CboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Content.ToString().Split('—')[0].Trim();
                ShowPanelsForType(type);
            }
        }

        // ── Panel + address ComboBox update ───────────────────────────────────
        private void ShowPanelsForType(string type)
        {
            if (PanelShared == null) return;

            PanelShared.Visibility  = Visibility.Visible;
            PanelInput.Visibility   = Visibility.Collapsed;
            PanelAnalog.Visibility  = Visibility.Collapsed;
            PanelAiOnly.Visibility  = Visibility.Collapsed;
            PanelOutput.Visibility  = Visibility.Collapsed;
            PanelAlarm.Visibility   = Visibility.Collapsed;

            bool isAlarm = type == "Alarm";
            PanelShared.Visibility = isAlarm ? Visibility.Collapsed : Visibility.Visible;

            switch (type)
            {
                case "AI":
                    PanelInput.Visibility  = Visibility.Visible;
                    PanelAnalog.Visibility = Visibility.Visible;
                    PanelAiOnly.Visibility = Visibility.Visible;
                    PopulateAddresses(AI_ADDRESSES);
                    break;
                case "AO":
                    PanelAnalog.Visibility = Visibility.Visible;
                    PanelOutput.Visibility = Visibility.Visible;
                    PopulateAddresses(AO_ADDRESSES);
                    break;
                case "DI":
                    PanelInput.Visibility = Visibility.Visible;
                    PopulateAddresses(DI_ADDRESSES);
                    break;
                case "DO":
                    PanelOutput.Visibility = Visibility.Visible;
                    PopulateAddresses(DO_ADDRESSES);
                    break;
                case "Alarm":
                    RefreshAlarmTagList();
                    PanelAlarm.Visibility = Visibility.Visible;
                    break;
            }
        }

        // ── Populate address ComboBox with available addresses only ───────────
        private void PopulateAddresses(List<string> pool)
        {
            var used = TagManager.Instance.Tags
                .Select(t => t.IOAddress)
                .ToHashSet();

            CboAddress.Items.Clear();
            foreach (var addr in pool)
                if (!used.Contains(addr))
                    CboAddress.Items.Add(addr);

            if (CboAddress.Items.Count > 0)
                CboAddress.SelectedIndex = 0;
        }

        // ── Alarm tag list ────────────────────────────────────────────────────
        private void RefreshAlarmTagList()
        {
            CboAlarmTag.Items.Clear();
            foreach (var tag in TagManager.Instance.Tags.OfType<AnalogInput>())
                CboAlarmTag.Items.Add(tag.Name);
        }

        // ── Add button ────────────────────────────────────────────────────────
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CboType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Content.ToString().Split('—')[0].Trim();
                try
                {
                    switch (type)
                    {
                        case "AI":    AddAnalogInput();  break;
                        case "AO":   AddAnalogOutput(); break;
                        case "DI":   AddDigitalInput(); break;
                        case "DO":   AddDigitalOutput(); break;
                        case "Alarm": AddAlarm();        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(TraceCategory.Error, $"AddWindow error: {ex.Message}");
                    MessageBox.Show(ex.Message, "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ── Validation helpers ────────────────────────────────────────────────

        private string RequireAddress()
        {
            if (CboAddress.SelectedItem == null)
                throw new Exception("No available I/O addresses for this tag type.");
            return CboAddress.SelectedItem.ToString();
        }

        private string RequireText(TextBox tb, string fieldName)
        {
            string v = tb.Text.Trim();
            if (string.IsNullOrEmpty(v))
                throw new Exception($"{fieldName} is required.");
            return v;
        }

        private double RequireDouble(TextBox tb, string fieldName)
        {
            // InvariantCulture ensures dot is always the decimal separator,
            // regardless of the OS locale (e.g. Serbian uses comma by default)
            if (!double.TryParse(tb.Text.Trim(),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                throw new Exception($"{fieldName} must be a valid number (use '.' as decimal separator).");
            return v;
        }

        private int RequireInt(TextBox tb, string fieldName)
        {
            if (!int.TryParse(tb.Text, out int v) || v <= 0)
                throw new Exception($"{fieldName} must be a positive integer.");
            return v;
        }

        // Validates that a TextBox contains exactly "0" or "1" — nothing else.
        // Returns the equivalent bool (0 → false, 1 → true).
        private bool RequireDigitalValue(TextBox tb, string fieldName)
        {
            string v = tb.Text.Trim();
            if (v == "0") return false;
            if (v == "1") return true;
            throw new Exception($"{fieldName} must be 0 or 1.");
        }

        private void CheckNameUnique(string name)
        {
            if (TagManager.Instance.Tags.Any(t => t.Name == name))
                throw new Exception($"A tag named '{name}' already exists.");
        }

        // ── Tag factory methods ───────────────────────────────────────────────

        private void AddAnalogInput()
        {
            string name = RequireText(TxtName, "Tag Name");
            CheckNameUnique(name);

            var tag = new AnalogInput
            {
                Name        = name,
                TagType     = "AI",
                Description = TxtDescription.Text.Trim(),
                IOAddress   = RequireAddress(),
                ScanTime    = RequireInt(TxtScanTime, "Scan Time"),
                ScanEnabled = ChkScanEnabled.IsChecked == true,
                LowLimit    = RequireDouble(TxtLow, "Low Limit"),
                HighLimit   = RequireDouble(TxtHigh, "High Limit"),
                Units       = TxtUnits.Text.Trim(),
                Deadband    = RequireDouble(TxtDeadband, "Deadband"),
                Hysteresis  = RequireDouble(TxtHysteresis, "Hysteresis"),
            };

            if (tag.HighLimit <= tag.LowLimit)
                throw new Exception("High Limit must be greater than Low Limit.");

            TagManager.Instance.AddTag(tag);
        }

        private void AddAnalogOutput()
        {
            string name = RequireText(TxtName, "Tag Name");
            CheckNameUnique(name);

            double init = RequireDouble(TxtInitialValue, "Initial Value");

            var tag = new AnalogOutput
            {
                Name         = name,
                TagType      = "AO",
                Description  = TxtDescription.Text.Trim(),
                IOAddress    = RequireAddress(),
                LowLimit     = RequireDouble(TxtLow, "Low Limit"),
                HighLimit    = RequireDouble(TxtHigh, "High Limit"),
                Units        = TxtUnits.Text.Trim(),
                InitialValue = init,
                CurrentValue = init,
            };

            TagManager.Instance.AddTag(tag);
        }

        private void AddDigitalInput()
        {
            string name = RequireText(TxtName, "Tag Name");
            CheckNameUnique(name);

            var tag = new DigitalInput
            {
                Name        = name,
                TagType     = "DI",
                Description = TxtDescription.Text.Trim(),
                IOAddress   = RequireAddress(),
                ScanTime    = RequireInt(TxtScanTime, "Scan Time"),
                ScanEnabled = ChkScanEnabled.IsChecked == true,
            };

            TagManager.Instance.AddTag(tag);
        }

        private void AddDigitalOutput()
        {
            string name = RequireText(TxtName, "Tag Name");
            CheckNameUnique(name);

            // Only 0 or 1 accepted — RequireDigitalValue throws on anything else
            bool initVal = RequireDigitalValue(TxtInitialValue, "Initial Value");

            var tag = new DigitalOutput
            {
                Name         = name,
                TagType      = "DO",
                Description  = TxtDescription.Text.Trim(),
                IOAddress    = RequireAddress(),
                InitialValue = initVal,
                CurrentValue = initVal,
            };

            TagManager.Instance.AddTag(tag);
        }

        private void AddAlarm()
        {
            if (CboAlarmTag.SelectedItem == null)
                throw new Exception("Select an AI tag to attach this alarm to.");

            double limit = RequireDouble(TxtAlarmLimit, "Limit Value");

            var alarm = new Alarm
            {
                TagName     = CboAlarmTag.SelectedItem.ToString(),
                Limit       = limit,
                IsHighAlarm = CboAlarmDir.SelectedIndex == 0,
                Message     = TxtAlarmMsg.Text.Trim(),
                State       = AlarmState.Inactive,
            };

            TagManager.Instance.AddAlarm(alarm);
        }
    }
}
