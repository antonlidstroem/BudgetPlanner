using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BudgetPlanner.WPF.ViewModels;

namespace BudgetPlanner.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TransactionsViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new TransactionsViewModel();
            DataContext = viewModel;
            //Loaded += TransactionsView_Loaded;
        }

        //private async void TransactionsView_Loaded(object sender, RoutedEventArgs e)
        //{
        //}

        //private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        //{
        //}
    }
}