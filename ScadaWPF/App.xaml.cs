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

            // Log application start
            Logger.Log(TraceCategory.Login, "APPLICATION_START");

            // Wire up global exception handler so errors are always logged
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                Logger.Log(TraceCategory.Error,
                    $"UnhandledException: {ex.ExceptionObject}");

            DispatcherUnhandledException += (s, ex) =>
            {
                Logger.Log(TraceCategory.Error,
                    $"DispatcherException: {ex.Exception.Message}");
                ex.Handled = true;  // prevent crash; show message instead
                MessageBox.Show($"Unexpected error:\n{ex.Exception.Message}",
                    "SCADA Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Touch TagManager.Instance to trigger the constructor:
            //   • loads tags/alarms from DB
            //   • starts all scan threads
            var _ = TagManager.Instance;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop the PLC simulator cleanly on exit
            new PLC().StopSimulator();
            Logger.Log(TraceCategory.Login, "APPLICATION_EXIT");
            base.OnExit(e);
        }
    }
}
