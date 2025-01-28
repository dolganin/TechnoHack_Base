using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using System.Linq;

namespace SignalLabelingApp.ViewModels
{
    public class MessageWindowViewModel : ObservableObject
    {
        private string _message = string.Empty; // Инициализация значением по умолчанию

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ICommand CloseCommand { get; }

        public MessageWindowViewModel()
        {
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void CloseWindow()
        {
            // Закрыть окно
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Получаем активное окно и закрываем его
                var activeWindow = desktop.Windows.FirstOrDefault(w => w.IsActive);
                activeWindow?.Close();
            }
        }
    }
}
