using System.Windows.Controls;

using CRUDApp.ViewModels;

namespace CRUDApp.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }
    }
}
