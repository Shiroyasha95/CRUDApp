using System.Windows.Controls;

using CRUDApp.ViewModels;

namespace CRUDApp.Views
{
    public partial class DataGridPage : Page
    {
        public DataGridPage(DataGridViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
