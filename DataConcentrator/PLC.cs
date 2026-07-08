using System.Collections.Generic;
using System.Threading;
using PLCSimulator;

namespace DataConcentrator
{
    // PLC singleton omotač
    // Čuva instancu PLCSimulatorManager i mapu nitova za skeniranje tagova.
    public class PLC
    {
        private static PLCSimulatorManager instance;

        // Rečnik: ime taga → njegova nit za skeniranje
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
