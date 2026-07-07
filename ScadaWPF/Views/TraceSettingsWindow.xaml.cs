using System.Windows;
using ScadaWPF.ViewModels;

namespace ScadaWPF.Views
{
    // ─── TraceSettingsWindow code-behind ──────────────────────────────────────
    // Almost all logic lives in TraceSettingsViewModel.
    // This file just wires up the DataContext and closes the window.
    public partial class TraceSettingsWindow : Window
    {
        public TraceSettingsWindow()
        {
            InitializeComponent();

            // ViewModel reads current bit states from Logger on construction
            DataContext = new TraceSettingsViewModel();
        }

        // The window can be closed via the system X button; no explicit close
        // button is needed because every change persists immediately via
        // Logger.SetBit → SaveTraceword.
    }
}
