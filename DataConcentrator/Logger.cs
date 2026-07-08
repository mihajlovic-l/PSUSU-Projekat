using System;
using System.IO;

namespace DataConcentrator
{
    // Kategorije za logovanje kao bitovi (0-7). Traceword je bitmask.
    public enum TraceCategory
    {
        Login        = 0,   // bit 0
        AlarmAck     = 1,   // bit 1
        TagAdd       = 2,   // bit 2
        Update    = 3,   // bit 3
        ImportExport = 4,   // bit 4
        AlarmRaised  = 5,   // bit 5
        TagWrite     = 6,   // bit 6
        Error        = 7    // bit 7
    }

    // Logger: statička klasa za centralno beleženje događaja.
    // Pisanje u fajl je sa zaključavanjem radi bezbednosti niti.
    public static class Logger
    {
        // Putanje fajlova (u direktorijumu aplikacije)
        private static readonly string LogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.log");

        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceword.cfg");

        // Lock za bezbedan pristup fajlu iz više niti
        private static readonly object _lock = new object();

        // Traceword: učitava se pri startu i čuva pri promeni.
        private static int _traceword;

        public static int Traceword
        {
            get { return _traceword; }
            private set
            {
                _traceword = value;
                SaveTraceword();                 // odmah sačuvaj na disk
            }
        }

        // Statički konstruktor koji učitava inicijalno stanje
        static Logger()
        {
            LoadTraceword();
        }

        // Javni API

        /// <summary>
        /// Upis u system.log ako je kategorija uključena u traceword.
        /// </summary>
        public static void Log(TraceCategory category, string message)
        {
            // Proveri da li je bit za ovu kategoriju uključen
            if (!IsBitSet(category)) return;

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{category.ToString().ToUpper()}] {message}";

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(LogPath, line + Environment.NewLine);
                }
                catch
                {
                    // Ako logovanje ne uspe, aplikacija ne sme da padne
                }
            }
        }

        /// <summary>
        /// Uključi/isključi kategoriju i sačuvaj traceword.
        /// </summary>
        public static void SetBit(TraceCategory category, bool enabled)
        {
            int mask = 1 << (int)category;

            if (enabled)
                Traceword = _traceword | mask;      // postavi bit
            else
                Traceword = _traceword & ~mask;     // ukloni bit
        }

        /// <summary>
        /// Proveri da li je kategorija uključena.
        /// </summary>
        public static bool IsBitSet(TraceCategory category)
        {
            int mask = 1 << (int)category;
            return (_traceword & mask) != 0;
        }

        // Persistencija traceword-a

        private static void LoadTraceword()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string text = File.ReadAllText(ConfigPath).Trim();
                    if (int.TryParse(text, out int value))
                    {
                        _traceword = value;
                        return;
                    }
                }
            }
            catch { /* koristi podrazumevanu vrednost */ }

            // Podrazumevano: Error + TagAdd
            _traceword = (1 << (int)TraceCategory.Error) | (1 << (int)TraceCategory.TagAdd);
            SaveTraceword();
        }

        private static void SaveTraceword()
        {
            try
            {
                File.WriteAllText(ConfigPath, _traceword.ToString());
            }
            catch { /* nije kritično — traceword ostaje u memoriji */ }
        }
    }
}
