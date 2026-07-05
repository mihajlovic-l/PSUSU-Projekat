using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // ─── Base Tag ───────────────────────────────────────────────────────────────
    // All four tag types inherit from this. INotifyPropertyChanged lets WPF
    // data-bindings react to value changes automatically.
    public class Tag : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private string ioAddress;

        [Key]
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged("Description"); }
        }

        // I/O address maps this tag to a slot in the PLC simulator
        public string IOAddress
        {
            get { return ioAddress; }
            set { ioAddress = value; OnPropertyChanged("IOAddress"); }
        }

        // Discriminator column so EF can store all tag subtypes in one table (TPH)
        public string TagType { get; set; }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }

    // ─── Input Tags (share scan-related fields) ──────────────────────────────
    public abstract class InputTag : Tag
    {
        private bool scanEnabled = true;

        // Scan interval in milliseconds
        public int ScanTime { get; set; } = 1000;

        // When false the background thread skips reading this tag
        public bool ScanEnabled
        {
            get { return scanEnabled; }
            set { scanEnabled = value; OnPropertyChanged("ScanEnabled"); }
        }
    }

    // ─── Analog Input ────────────────────────────────────────────────────────
    public class AnalogInput : InputTag
    {
        private double currentValue;

        public double LowLimit  { get; set; } = 0;
        public double HighLimit { get; set; } = 100;
        public string Units     { get; set; } = "";

        // Minimum change required before the value is considered updated
        public double Deadband   { get; set; } = 0;

        // Amount the value must exceed an alarm threshold before the alarm fires
        // (prevents rapid on/off toggling near the boundary)
        public double Hysteresis { get; set; } = 0;

        // Live value — changing this triggers property-change events so the UI updates
        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }

    // ─── Analog Output ───────────────────────────────────────────────────────
    public class AnalogOutput : Tag
    {
        public double LowLimit     { get; set; } = 0;
        public double HighLimit    { get; set; } = 100;
        public string Units        { get; set; } = "";
        public double InitialValue { get; set; } = 0;

        private double currentValue;
        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }

    // ─── Digital Input ───────────────────────────────────────────────────────
    public class DigitalInput : InputTag
    {
        private bool currentValue;
        public bool CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }

    // ─── Digital Output ──────────────────────────────────────────────────────
    public class DigitalOutput : Tag
    {
        public bool InitialValue { get; set; } = false;

        private bool currentValue;
        public bool CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }
}
