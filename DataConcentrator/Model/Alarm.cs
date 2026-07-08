using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Definicija alarma
    // Alarm je vezan za jedan AnalogInput tag (preko TagName).
    public class Alarm : INotifyPropertyChanged
    {
        private AlarmState state = AlarmState.Inactive;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Ime AI taga koji ovaj alarm prati
        public string TagName { get; set; }

        // Granična vrednost koja aktivira alarm
        public double Limit { get; set; }

        // True = alarm se aktivira kad vrednost pređe iznad granice
        // False = alarm se aktivira kad vrednost padne ispod granice
        public bool IsHighAlarm { get; set; } = true;

        public string Message { get; set; } = "";

        // Stanja alarma: Inactive → Active → Acknowledged
        public AlarmState State
        {
            get { return state; }
            set { state = value; OnPropertyChanged("State"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string p) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    // Enum stanja alarma
    public enum AlarmState
    {
        Inactive,       // vrednost je u normalnom opsegu
        Active,         // granica pređena, korisnik još nije potvrdio (crveno)
        Acknowledged    // korisnik potvrdio, ali vrednost i dalje van opsega (žuto)
    }

    // Zapis aktiviranog alarma koji se upisuje u bazu prilikom pokretanja alarma.
    public class ActivatedAlarm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int    AlarmId     { get; set; }   // koja definicija alarma je aktivirana
        public string TagName     { get; set; }   // koji AI tag je bio van opsega
        public string Message     { get; set; }   // poruka alarma u trenutku aktiviranja
        public DateTime Timestamp { get; set; }   // kada se desilo
    }

    // Zapis vrednosti taga koji se čuva pri promeni vrednosti AI taga.
    public class TagValueRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string   TagName   { get; set; }
        public double   Value     { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
