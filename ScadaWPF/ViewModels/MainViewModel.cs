using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaWPF.ViewModels
{
    // ─── MainViewModel ────────────────────────────────────────────────────────
    // Sits between MainWindow.xaml and the TagManager service.
    // All collections here are the same ObservableCollections that TagManager
    // owns, so WPF gets live updates whenever scan threads modify values.
    public class MainViewModel : INotifyPropertyChanged
    {
        // Direct references to TagManager's collections (not copies)
        public ObservableCollection<Tag>            Tags   => TagManager.Instance.Tags;
        public ObservableCollection<Alarm>          Alarms => TagManager.Instance.Alarms;

        // ── Selected item tracking ────────────────────────────────────────────
        private Tag _selectedTag;
        public Tag SelectedTag
        {
            get => _selectedTag;
            set { _selectedTag = value; OnPropertyChanged(nameof(SelectedTag)); }
        }

        private Alarm _selectedAlarm;
        public Alarm SelectedAlarm
        {
            get => _selectedAlarm;
            set { _selectedAlarm = value; OnPropertyChanged(nameof(SelectedAlarm)); }
        }

        // ── Constructor ───────────────────────────────────────────────────────
        public MainViewModel()
        {
            // Subscribe to alarm events from the scan threads.
            // AlarmRaised fires on a background thread, so we need Dispatcher.
            TagManager.Instance.AlarmRaised += OnAlarmRaised;
        }

        // ── Alarm event handler ───────────────────────────────────────────────
        private void OnAlarmRaised(object sender, AlarmRaisedEventArgs e)
        {
            // Read the alarm from the DB on the UI thread and show it
            Application.Current.Dispatcher.Invoke(() =>
            {
                var alarm = Alarms.FirstOrDefault(a => a.Id == e.AlarmId);
                if (alarm != null)
                {
                    // Flash message to inform the user — the row colour changes via binding
                    MessageBox.Show(
                        $"⚠ Alarm triggered!\n\nTag: {alarm.TagName}\nMessage: {alarm.Message}",
                        "SCADA Alarm",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            });
        }

        // ── Actions ───────────────────────────────────────────────────────────

        public void RemoveSelectedTag()
        {
            if (SelectedTag == null) return;
            TagManager.Instance.RemoveTag(SelectedTag.Name);
        }

        public void AcknowledgeSelectedAlarm()
        {
            if (SelectedAlarm == null) return;
            TagManager.Instance.AcknowledgeAlarm(SelectedAlarm.Id);
        }

        public void ToggleScan(string tagName, bool enabled)
        {
            TagManager.Instance.SetScanEnabled(tagName, enabled);
        }

        public void WriteToTag(string tagName, string valueText)
        {
            var tag = Tags.FirstOrDefault(t => t.Name == tagName);
            if (tag == null) return;

            try
            {
                if (tag is AnalogOutput)
                {
                    if (double.TryParse(valueText.Trim(),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out double dv))
                        TagManager.Instance.WriteAnalogOutput(tagName, dv);
                    else
                        MessageBox.Show("Enter a valid numeric value.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (tag is DigitalOutput)
                {
                    // Only exactly "0" or "1" accepted for digital outputs
                    if (valueText == "0")
                        TagManager.Instance.WriteDigitalOutput(tagName, false);
                    else if (valueText == "1")
                        TagManager.Instance.WriteDigitalOutput(tagName, true);
                    else
                        MessageBox.Show("Digital output only accepts 0 or 1.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"WriteToTag failed: {ex.Message}");
                MessageBox.Show($"Write failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GenerateReport()
        {
            try
            {
                string content = TagManager.Instance.GenerateReport();
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                File.WriteAllText(path, content);
                Logger.Log(TraceCategory.ImportExport, $"REPORT_GENERATED path={path}");
                MessageBox.Show($"Report saved to:\n{path}", "Report Generated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"GenerateReport failed: {ex.Message}");
                MessageBox.Show($"Report generation failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
