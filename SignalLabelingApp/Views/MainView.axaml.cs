using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Interactivity;
using SignalLabelingApp.Classes;
using System.Threading.Tasks;
using Label = SignalLabelingApp.Classes.Label;

namespace SignalLabelingApp.Views
{
    public partial class MainView : UserControl
    {
        private int classNumber = 0;

        public MainView()
        {
            InitializeComponent();
            Globals.MainEditorControl = EditorZone;
            Globals.AllDatasetSamples.CollectionChanged += (_, __) => UpdateDatasetSamplesView();

            OpenFileMenuItem.PointerPressed += OpenFileMenuItem_PointerPressed;
            SaveMenuItem.PointerPressed += SaveMenuItem_PointerPressed;
            SaveButton.Click += OnSaveButtonClick;
            IncreaseClassNumberButton.Click += OnIncreaseClassNumberClick;
            DecreaseClassNumberButton.Click += OnDecreaseClassNumberClick;
        }

        private void OpenFileMenuItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var Editor = FileSaverLoader.LoadEditorFromFile();
            EditorZone.Child = Editor;
            Globals.CurrentEditor = Editor;
        }

        private void SaveMenuItem_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            string dirPath = Task.Run(async () => await FileSaverLoader.GetSaveDirPathAsync()).GetAwaiter().GetResult();

            foreach (DatasetSample sample in Globals.AllDatasetSamples)
            {
                string fileName = FileSaverLoader.GenerateUniqueFileName("sample", ".json", dirPath);
                FileSaverLoader.SaveToJson<DatasetSample>(sample, fileName, dirPath);
            }
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            var classNumber = ClassNumberTextBox.Text;
            var markupType = (MarkupTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var adaptiveSize = AdaptiveSizeTextBox.Text;

            // Логика сохранения данных
            SaveData(classNumber, markupType, adaptiveSize);
        }

        private void SaveData(string classNumber, string markupType, string adaptiveSize)
        {
            if (int.TryParse(classNumber, out int parsedClassNumber) && double.TryParse(adaptiveSize, out double parsedAdaptiveSize))
            {
                var newLabel = new SignalClassificationLabel
                {
                    ObjectClass = parsedClassNumber,
                    MarkupType = markupType,
                    AdaptiveSize = parsedAdaptiveSize
                };

                var newSample = new DatasetSample
                {
                    Label = newLabel
                };

                Globals.AllDatasetSamples.Add(newSample);
            }
            else
            {
                // Обработка ошибок ввода
                var messageWindow = new MessageWindow("Please enter valid values for class number and adaptive size.");
                messageWindow.Show();
            }
        }

        private void UpdateDatasetSamplesView()
        {
            CreatedLabels.Children.Clear();

            foreach (var sample in Globals.AllDatasetSamples)
            {
                if (sample.Label == null) continue;

                Label label = sample.Label;

                var labelBlock = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0),
                };

                var border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                };

                border.Child = labelBlock;

                if (label is SignalClassificationLabel classificationLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Classification" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {classificationLabel.ObjectStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {classificationLabel.ObjectEndPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Class: {classificationLabel.ObjectClass}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Markup Type: {classificationLabel.MarkupType}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Adaptive Size: {classificationLabel.AdaptiveSize}" });
                }
                else if (label is SignalDetectionLabel detectionLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Detection" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {detectionLabel.SignalStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {detectionLabel.SignalEndPos}" });

                    foreach (var obj in detectionLabel.Objects)
                    {
                        labelBlock.Children.Add(new TextBlock { Text = $"Object: Start={obj.X}, End={obj.W}, Class={obj.Class}" });
                    }
                }
                else if (label is SignalSegmentationLabel segmentationLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Segmentation" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {segmentationLabel.ObjectStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {segmentationLabel.ObjectEndPos}" });
                }

                var deleteButton = new Button
                {
                    Content = "Delete",
                    Margin = new Thickness(5),
                };

                deleteButton.Click += (s, e) =>
                {
                    Globals.AllDatasetSamples.Remove(sample);
                };

                labelBlock.Children.Add(deleteButton);

                CreatedLabels.Children.Add(border);
            }
        }

        private void OnIncreaseClassNumberClick(object sender, RoutedEventArgs e)
        {
            classNumber++;
            ClassNumberTextBox.Text = classNumber.ToString();
        }

        private void OnDecreaseClassNumberClick(object sender, RoutedEventArgs e)
        {
            if (classNumber > 0)
            {
                classNumber--;
                ClassNumberTextBox.Text = classNumber.ToString();
            }
        }
    }
}