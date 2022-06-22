using System.Windows.Controls;

using CRUDApp.ViewModels;

namespace CRUDApp.Views
{
    public partial class ListDetailsPage : Page
    {
        public ListDetailsPage(ListDetailsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
