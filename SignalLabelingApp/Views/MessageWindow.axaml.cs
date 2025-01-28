using Avalonia.Controls;
using SignalLabelingApp.ViewModels;

namespace SignalLabelingApp.Views
{
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
            DataContext = new MessageWindowViewModel();
        }

        public MessageWindow(string message) : this()
        {
            (DataContext as MessageWindowViewModel).Message = message;
        }
    }
}
