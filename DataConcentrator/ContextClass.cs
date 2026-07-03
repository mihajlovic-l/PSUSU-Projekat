using System.Data.Entity;

namespace DataConcentrator
{
    // ─── Entity Framework DbContext ───────────────────────────────────────────
    // Singleton pattern keeps one shared connection for the app's lifetime.
    // EF will create the database on first use (Code-First).
    // Three tables as required by the spec: Tags, Alarms, ActivatedAlarms.
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

        // Tags table — stores all AI/AO/DI/DO rows (Table-Per-Hierarchy via TagType)
        public DbSet<Tag>            Tags            { get; set; }

        // Alarms table — alarm definitions linked to AI tags
        public DbSet<Alarm>          Alarms          { get; set; }

        // ActivatedAlarms table — audit log of every alarm event
        public DbSet<ActivatedAlarm> ActivatedAlarms { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // TPH (Table-Per-Hierarchy): all tag subclasses share the Tags table.
            // EF uses the TagType string column to know which subclass to instantiate.
            modelBuilder.Entity<Tag>()
                .Map<AnalogInput>  (m => m.Requires("TagType").HasValue("AI"))
                .Map<AnalogOutput> (m => m.Requires("TagType").HasValue("AO"))
                .Map<DigitalInput> (m => m.Requires("TagType").HasValue("DI"))
                .Map<DigitalOutput>(m => m.Requires("TagType").HasValue("DO"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
