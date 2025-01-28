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
        public MainView()
        {
            InitializeComponent();
            Globals.MainEditorControl = EditorZone;
            //Globals.CurrentEditorMetadata = EditorMetadata;
            Globals.AllDatasetSamples.CollectionChanged += (_, __) => UpdateDatasetSamplesView();
            

            OpenFileMenuItem.PointerPressed += OpenFileMenuItem_PointerPressed;
            SaveMenuItem.PointerPressed += SaveMenuItem_PointerPressed;


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

            foreach(DatasetSample sample in Globals.AllDatasetSamples)
            {
                string fileName = FileSaverLoader.GenerateUniqueFileName("sample", ".json", dirPath);
                FileSaverLoader.SaveToJson<DatasetSample>(sample, fileName, dirPath);
            }

        }

        private void UpdateDatasetSamplesView(/*object sender, RoutedEventArgs e*/)
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
                    BorderBrush = Brushes.Black, // Цвет рамки
                    BorderThickness = new Thickness(1), // Толщина рамки  
                };

                border.Child = labelBlock;

                if (label is SignalClassificationLabel classificationLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Classification" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {classificationLabel.ObjectStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {classificationLabel.ObjectEndPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Class: {classificationLabel.ObjectClass}" });
                }
                else if (label is SignalDetectionLabel detectionLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Detection" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {detectionLabel.SignalStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {detectionLabel.SignalEndPos}" });

                    foreach (var obj in detectionLabel.Objects)
                    {
                        labelBlock.Children.Add(new TextBlock { Text = $"Object: Start={obj.X}\n, End={obj.W}, \nClass={obj.Class}" });
                    }
                }
                else if (label is SignalSegmentationLabel segmentationLabel)
                {
                    labelBlock.Children.Add(new TextBlock { Text = $"Type: Segmentation" });
                    labelBlock.Children.Add(new TextBlock { Text = $"Start: {segmentationLabel.ObjectStartPos}" });
                    labelBlock.Children.Add(new TextBlock { Text = $"End: {segmentationLabel.ObjectEndPos}" });
                }

                //CreatedLabels.Children.Add(border);
                //Button deleteButton = new Button { Content = "Удалить", Tag = elementPanel };

                // Добавление обработчика события для кнопки удаления
                //deleteButton.Click += DeleteButton_Click;

              
                //elementPanel.Children.Add(deleteButton);
                //elementCount++;
                //private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Получение панели, содержащей кнопку удаления
            /*Button deleteButton = (Button)sender;
            StackPanel elementPanel = (StackPanel)deleteButton.Tag;

            // Удаление элемента из стек-панели
            editableStackPanel.Children.Remove(elementPanel);*/
        }


        // Добавляем кнопку удаления
        var deleteButton = new Button
        {
            Content = "Delete",
            Margin = new Thickness(5),
        };

        // Подписываемся на событие нажатия на кнопку удаления
        deleteButton.Click += (s, e) =>
        {
            Globals.AllDatasetSamples.Remove(sample);
        };

        labelBlock.Children.Add(deleteButton);

        CreatedLabels.Children.Add(border);
                
            }
        }

    }
    
}
 