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
                // Обработчик клика на кнопку "Закрыть"
        private void CloseCommand(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Закрытие окна
            this.Close();
        }
    }
}
