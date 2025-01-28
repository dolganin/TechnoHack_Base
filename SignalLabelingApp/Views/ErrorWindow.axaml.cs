using Avalonia.Controls;
using SignalLabelingApp.ViewModels;

namespace SignalLabelingApp.Views
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
            DataContext = new ErrorWindowViewModel();
        }
    }
}
