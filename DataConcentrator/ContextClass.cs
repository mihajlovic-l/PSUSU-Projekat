using System.Data.Entity;

namespace DataConcentrator
{
    // ─── Entity Framework DbContext ───────────────────────────────────────────
    // Using one DbSet per concrete tag type instead of TPH through the base Tag.
    // This is simpler and avoids EF ObjectStateEntry tracking issues with
    // abstract base classes and discriminator columns.
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

        // One DbSet per concrete tag type — each maps to its own table
        public DbSet<AnalogInput>    AnalogInputs    { get; set; }
        public DbSet<AnalogOutput>   AnalogOutputs   { get; set; }
        public DbSet<DigitalInput>   DigitalInputs   { get; set; }
        public DbSet<DigitalOutput>  DigitalOutputs  { get; set; }

        // Alarms and audit log
        public DbSet<Alarm>          Alarms          { get; set; }
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        // History of AI tag values — used for report generation
        public DbSet<TagValueRecord> TagValueRecords { get; set; }
    }
}
