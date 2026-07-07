using System;
using System.Windows;
using DataConcentrator;

namespace ScadaWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            // Zabeleži start aplikacije
            Logger.Log(TraceCategory.Login, "APPLICATION_START");

            // Globalni handler izuzetaka koji loguje greške
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                Logger.Log(TraceCategory.Error,
                    $"UnhandledException: {ex.ExceptionObject}");

            DispatcherUnhandledException += (s, ex) =>
            {
                Logger.Log(TraceCategory.Error,
                    $"DispatcherException: {ex.Exception.Message}");
                ex.Handled = true;  // spreči pad aplikacije; prikaži poruku umesto toga
                MessageBox.Show($"Unexpected error:\n{ex.Exception.Message}",
                    "SCADA Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Inicijalizuj TagManager da učita podatke i pokrene niti
            var _ = TagManager.Instance;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Zaustavi PLC simulator pri izlasku
            new PLC().StopSimulator();
            Logger.Log(TraceCategory.Login, "APPLICATION_EXIT");
            base.OnExit(e);
        }
    }
}
