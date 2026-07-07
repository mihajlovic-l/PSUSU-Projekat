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

        // Foreign key — name of the AI tag this alarm watches
        public string TagName { get; set; }

        // The threshold value that triggers the alarm
        public double Limit { get; set; }

        // True = alarm fires when value goes ABOVE the limit
        // False = alarm fires when value drops BELOW the limit
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
        Inactive,       // value is within normal range
        Active,         // alarm threshold crossed, user hasn't seen it yet (red)
        Acknowledged    // user clicked ACK, but value is still out of range (yellow)
    }

    // Zapis aktiviranog alarma koji se upisuje u bazu prilikom pokretanja alarma.
    public class ActivatedAlarm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int    AlarmId     { get; set; }   // which alarm definition triggered
        public string TagName     { get; set; }   // the AI tag that was out of range
        public string Message     { get; set; }   // alarm message at the time of firing
        public DateTime Timestamp { get; set; }   // when it happened
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
