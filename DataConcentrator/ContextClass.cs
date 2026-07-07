using System.Data.Entity;

namespace DataConcentrator
{
    // DbContext za Entity Framework
    // Jedan DbSet po konkretnom tipu taga radi jednostavnosti i stabilnosti.
    public class ContextClass : DbContext
    {
        private static ContextClass instance;

        public static ContextClass Instance
        {
            get
            {
                if (instance == null)
                    instance = new ContextClass();
                return instance;
            }
        }

        // Jedan DbSet po konkretnom tipu taga (mapira na svoju tabelu)
        public DbSet<AnalogInput>    AnalogInputs    { get; set; }
        public DbSet<AnalogOutput>   AnalogOutputs   { get; set; }
        public DbSet<DigitalInput>   DigitalInputs   { get; set; }
        public DbSet<DigitalOutput>  DigitalOutputs  { get; set; }

        // Alarmi i zapisnik aktivacija
        public DbSet<Alarm>          Alarms          { get; set; }
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        // Istorija vrednosti AI tagova (koristi se za izveštaje)
        public DbSet<TagValueRecord> TagValueRecords { get; set; }
    }
}
