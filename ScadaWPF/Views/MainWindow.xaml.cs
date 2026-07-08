using System.Windows;
using DataConcentrator;
using ScadaWPF.ViewModels;

namespace ScadaWPF.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
            Logger.Log(TraceCategory.Login, "MAIN_WINDOW_OPEN");
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddWindow { Owner = this };
            win.ShowDialog();
        }

        private void BtnUpdateTag_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedTag == null)
            {
                MessageBox.Show("Select a tag to update.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new UpdateWindow(_vm.SelectedTag) { Owner = this };
            win.ShowDialog();
        }

        private void BtnRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedTag == null)
            {
                MessageBox.Show("Select a tag to remove.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Remove tag '{_vm.SelectedTag.Name}'?",
                "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                _vm.RemoveSelectedTag();
        }

        private void BtnWrite_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedTag == null)
            {
                MessageBox.Show("Select an output tag first.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _vm.WriteToTag(_vm.SelectedTag.Name, TxtWriteValue.Text);
        }

        private void BtnToggleScan_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedTag is InputTag input)
                _vm.ToggleScan(input.Name, !input.ScanEnabled);
            else
                MessageBox.Show("Select an input tag (AI or DI) to toggle scanning.",
                    "Not an input tag", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            _vm.GenerateReport();
        }

        // Otvara prozor sa detaljima za izabrani tag
        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedTag == null)
            {
                MessageBox.Show("Select a tag to view its details.", "No selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new DetailsWindow(_vm.SelectedTag) { Owner = this };
            win.ShowDialog();
        }

        private void BtnTrace_Click(object sender, RoutedEventArgs e)
        {
            var win = new TraceSettingsWindow { Owner = this };
            win.ShowDialog();
        }

        // Klik na prazno mesto u listi tagova -> poništi selekciju
        private void TagList_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Proveri klik — ako nije pogodio stavku liste, poništi selekciju
            var hit = System.Windows.Media.VisualTreeHelper.HitTest(TagList, e.GetPosition(TagList));
            if (hit == null) return;

            var item = FindAncestor<System.Windows.Controls.ListViewItem>(
                hit.VisualHit as System.Windows.DependencyObject);

            if (item == null)
            {
                TagList.SelectedItem = null;
                _vm.SelectedTag = null;
            }
        }

        // Traži roditeljski element datog tipa u vizuelnom stablu
        private static T FindAncestor<T>(System.Windows.DependencyObject current)
            where T : System.Windows.DependencyObject
        {
            while (current != null)
            {
                if (current is T t) return t;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
