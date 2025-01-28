using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SignalLabelingApp.Classes;
using SignalLabelingApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace SignalLabelingApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObjectSelectionManager objectSelectionManager;
        private StationData currentStationData;

        // Свойства, которые могут быть привязаны к UI
        public ObservableCollection<string> StationNames { get; set; }
        public double ScaleX { get; set; }
        public string ObjectClassId { get; set; }
        public Canvas MyCanvas { get; set; }  // Добавлено для Canvas

        // Свойства для работы с диалоговыми окнами
        public MessageWindowViewModel MessageWindowViewModel { get; set; }

        // Новые свойства для работы с типами меток
        public ObservableCollection<LabelType> LabelTypes { get; set; }
        private LabelType _selectedLabelType;
        public LabelType SelectedLabelType
        {
            get => _selectedLabelType;
            set => SetProperty(ref _selectedLabelType, value);
        }

        // Свойство для адаптивного размера
        private bool _adaptiveSizeEnabled;
        public bool AdaptiveSizeEnabled
        {
            get => _adaptiveSizeEnabled;
            set => SetProperty(ref _adaptiveSizeEnabled, value);
        }

        // Команда для сохранения метки
        public IAsyncRelayCommand SaveCommand { get; }

        public MainWindowViewModel()
        {
            // Инициализация
            StationNames = new ObservableCollection<string>();
            ScaleX = 1.0;
            MessageWindowViewModel = new MessageWindowViewModel(); // Инициализация для использования

            // Инициализация типов меток
            LabelTypes = new ObservableCollection<LabelType>
            {
                LabelType.Classification,
                LabelType.Detection,
                LabelType.Segmentation
            };
            SelectedLabelType = LabelTypes.First();  // Установим тип по умолчанию

            // Инициализация команды для сохранения метки
            SaveCommand = new AsyncRelayCommand(SaveLabelAsync);
        }

        // Метод, который будет вызываться для инициализации ObjectSelectionManager после создания окна
        public void InitializeObjectSelectionManager(Canvas canvas)
        {
            MyCanvas = canvas;
            objectSelectionManager = new ObjectSelectionManager(MyCanvas); // Передаем Canvas в конструктор ObjectSelectionManager
        }

        // Метод загрузки данных
        public void LoadMiniseedFile(MiniseedFile miniseedFile)
        {
            StationNames.Clear();
            foreach (var stationData in miniseedFile.stationDataStructures)
            {
                StationNames.Add(stationData.StationName);
            }
        }

        // Метод для выбора станции
        public void SelectStation(string stationName)
        {
            var selectedStation = FindStationByName(stationName);
            if (selectedStation != null)
            {
                currentStationData = selectedStation;
                UpdateCanvasWidth();
                ShowMessage("Станция выбрана", "Вы успешно выбрали станцию.");
            }
        }

        // Метод для поиска станции по имени
        private StationData FindStationByName(string stationName)
        {
            return currentStationData; // Замените на реальный поиск
        }

        // Обновление масштаба на основе ScaleX
        public void UpdateScaleX(double newScaleX)
        {
            ScaleX = newScaleX;
            if (currentStationData != null)
            {
                UpdateCanvasWidth();
            }
        }

        // Метод для сохранения метки
        public async Task SaveLabelAsync()
        {
            if (objectSelectionManager != null && currentStationData != null)
            {
                objectSelectionManager.SaveSelection(); // Теперь вызывает метод SaveSelection
                await Task.Run(() =>
                {
                    SaveSelectionToFile("label_data.json");
                    ShowMessage("Метка сохранена", $"Метка типа {SelectedLabelType} успешно сохранена.");
                });
            }
            else
            {
                ShowMessage("Ошибка", "Не удалось сохранить метку.");
            }
        }

        // Метод для отображения диалогового окна с сообщением
        private void ShowMessage(string title, string message)
        {
            MessageWindowViewModel.Message = message;
            OpenMessageWindow(title);
        }

        // Метод для открытия диалогового окна с сообщением
        private void OpenMessageWindow(string title)
        {
            var messageWindow = new MessageWindow(title)
            {
                Title = title,
                Width = 300,
                Height = 150
            };

            var topWindow = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.FirstOrDefault();
            if (topWindow != null)
            {
                messageWindow.ShowDialog(topWindow);
            }
        }

        // Сохранение в файл
        private void SaveSelectionToFile(string filePath)
        {
            Console.WriteLine($"Saving selection to {filePath}");
        }

        // Обновление ширины холста при изменении масштаба
        private void UpdateCanvasWidth()
        {
            if (currentStationData != null)
            {
                int maxSamples = new[] {
                    currentStationData.Channel1?.data?.Count ?? 0,
                    currentStationData.Channel2?.data?.Count ?? 0,
                    currentStationData.Channel3?.data?.Count ?? 0
                }.Max();
                Console.WriteLine($"Canvas width updated to: {maxSamples * ScaleX}");
            }
        }
    }

    // Перечисление для типов меток
    public enum LabelType
    {
        Classification,
        Detection,
        Segmentation
    }
}
