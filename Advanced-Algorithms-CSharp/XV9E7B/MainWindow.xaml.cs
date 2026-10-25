using System.Windows;

namespace XV9E7B
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewModel; 
        }

        private void AddToOrders_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.AddToOrders();
        }
    }
}
