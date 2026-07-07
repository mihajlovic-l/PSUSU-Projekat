using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Osnovni Tag: zajedničke osobine svih tipova tagova.
    // INotifyPropertyChanged omogućava automatsko osvežavanje UI-a.
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

    // Ulazni tagovi: dele polja vezana za skeniranje
    public abstract class InputTag : Tag
    {
        private bool scanEnabled = true;

        // Interval skeniranja u milisekundama
        public int ScanTime { get; set; } = 1000;

        // Ako je false, nit za skeniranje preskače čitanje
        public bool ScanEnabled
        {
            get { return scanEnabled; }
            set { scanEnabled = value; OnPropertyChanged("ScanEnabled"); }
        }
    }

    // Analogni ulaz
    public class AnalogInput : InputTag
    {
        private double currentValue;

        public double LowLimit  { get; set; } = 0;
        public double HighLimit { get; set; } = 100;
        public string Units     { get; set; } = "";

        // Minimalna promena koja se smatra ažuriranjem (deadband)
        public double Deadband   { get; set; } = 0;

        // Histereza: dodatna margina da se spreči brzo uključenje/isključenje
        public double Hysteresis { get; set; } = 0;

        // Trenutna vrednost taga; promena emituje događaj za UI
        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }

    // Analogni izlaz
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

    // Digitalni ulaz
    public class DigitalInput : InputTag
    {
        private bool currentValue;
        public bool CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }
    }

    // Digitalni izlaz
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
