using System.Windows.Controls;

using CRUDApp.ViewModels;

namespace CRUDApp.Views
{
    public partial class BlankPage : Page
    {
        public BlankPage(BlankViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
