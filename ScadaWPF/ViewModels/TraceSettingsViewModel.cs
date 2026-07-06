using System.ComponentModel;
using DataConcentrator;

namespace ScadaWPF.ViewModels
{
    // ─── TraceSettingsViewModel ───────────────────────────────────────────────
    // Each public bool property maps to one bit in Logger.Traceword.
    // The TraceSettingsWindow binds each CheckBox.IsChecked to one of these.
    // Setting a property calls Logger.SetBit → persists to disk immediately.
    public class TraceSettingsViewModel : INotifyPropertyChanged
    {
        // ── One property per TraceCategory ────────────────────────────────────

        public bool LoginEnabled
        {
            get => Logger.IsBitSet(TraceCategory.Login);
            set
            {
                Logger.SetBit(TraceCategory.Login, value);
                Refresh();
            }
        }

        public bool AlarmAckEnabled
        {
            get => Logger.IsBitSet(TraceCategory.AlarmAck);
            set
            {
                Logger.SetBit(TraceCategory.AlarmAck, value);
                Refresh();
            }
        }

        public bool TagAddEnabled
        {
            get => Logger.IsBitSet(TraceCategory.TagAdd);
            set
            {
                Logger.SetBit(TraceCategory.TagAdd, value);
                Refresh();
            }
        }

        public bool UpdateEnabled
        {
            get => Logger.IsBitSet(TraceCategory.Update);
            set
            {
                Logger.SetBit(TraceCategory.Update, value);
                Refresh();
            }
        }

        public bool ImportExportEnabled
        {
            get => Logger.IsBitSet(TraceCategory.ImportExport);
            set
            {
                Logger.SetBit(TraceCategory.ImportExport, value);
                Refresh();
            }
        }

        public bool AlarmRaisedEnabled
        {
            get => Logger.IsBitSet(TraceCategory.AlarmRaised);
            set
            {
                Logger.SetBit(TraceCategory.AlarmRaised, value);
                Refresh();
            }
        }

        public bool TagWriteEnabled
        {
            get => Logger.IsBitSet(TraceCategory.TagWrite);
            set
            {
                Logger.SetBit(TraceCategory.TagWrite, value);
                Refresh();
            }
        }

        public bool ErrorEnabled
        {
            get => Logger.IsBitSet(TraceCategory.Error);
            set
            {
                Logger.SetBit(TraceCategory.Error, value);
                Refresh();
            }
        }

        // ── Traceword display ─────────────────────────────────────────────────
        // Shows the numeric value and its binary representation so the user
        // can see the bitmask in real time as they toggle checkboxes.

        public int Traceword => Logger.Traceword;

        // Binary string with leading zeros, e.g. "00100101"
        public string TracewordBinary =>
            System.Convert.ToString(Logger.Traceword, 2).PadLeft(8, '0');

        // ── Refresh helper ────────────────────────────────────────────────────
        // Called after every bit change to update the numeric/binary displays.
        private void Refresh()
        {
            OnPropertyChanged(nameof(Traceword));
            OnPropertyChanged(nameof(TracewordBinary));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
