using System.ComponentModel;
using DataConcentrator;

namespace ScadaWPF.ViewModels
{
    // TraceSettingsViewModel: svojstva mapiraju bitove u Logger.Traceword.
    // Promena svojstva poziva Logger.SetBit i odmah se čuva.
    public class TraceSettingsViewModel : INotifyPropertyChanged
    {
        // Po jedno svojstvo za svaku kategoriju logovanja

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

        // Prikaz traceword-a: brojčano i binarno

        public int Traceword => Logger.Traceword;

        // Binarni string sa vodećim nulama, npr. "00100101"
        public string TracewordBinary =>
            System.Convert.ToString(Logger.Traceword, 2).PadLeft(8, '0');

        // Pomoćna metoda za osvežavanje prikaza nakon promene bita
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
