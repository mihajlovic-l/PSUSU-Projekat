using System.Windows;
using ScadaWPF.ViewModels;

namespace ScadaWPF.Views
{
    // Kod iza prozora za podešavanje logovanja.
    // Logika je u ViewModel-u; ovde se samo postavlja DataContext.
    public partial class TraceSettingsWindow : Window
    {
        public TraceSettingsWindow()
        {
            InitializeComponent();

            // ViewModel reads current bit states from Logger on construction
            DataContext = new TraceSettingsViewModel();
        }

        // Prozor se može zatvoriti standardnim X; promene se odmah čuvaju.
    }
}
