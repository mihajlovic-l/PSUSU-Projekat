using System.Collections.Generic;
using System.Threading;
using PLCSimulator;

namespace DataConcentrator
{
    // ─── PLC Singleton Wrapper ────────────────────────────────────────────────
    // Keeps the skeleton's PLCSimulatorManager singleton pattern intact.
    // tagThreads dictionary is accessed by TagManager to track scan threads.
    public class PLC
    {
        private static PLCSimulatorManager instance;

        // Dictionary maps tag name → its scan thread
        public static Dictionary<string, Thread> tagThreads =
            new Dictionary<string, Thread>();

        public static PLCSimulatorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLCSimulatorManager();
                    instance.StartPLCSimulator();
                }
                return instance;
            }
        }

        public void StopSimulator()
        {
            instance?.Abort();
        }
    }
}
