using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SignalLabelingApp.ViewModels
{
    public partial class ErrorWindowViewModel : ObservableObject
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Команда для закрытия окна
        public IRelayCommand CloseCommand { get; }

        // Конструктор
        public ErrorWindowViewModel()
        {
            CloseCommand = new RelayCommand(CloseWindow);
        }

        // Метод для закрытия окна
        private void CloseWindow()
        {
            // Закрытие окна через механизм взаимодействия с View
            CloseWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        // Событие для запроса закрытия окна
        public event EventHandler CloseWindowRequested;

        // Метод для установки сообщения об ошибке
        public void SetErrorMessage(string message)
        {
            ErrorMessage = message;
        }
    }
}