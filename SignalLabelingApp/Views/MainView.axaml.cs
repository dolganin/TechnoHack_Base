using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Interactivity;
using SignalLabelingApp.Classes;
using System.Threading.Tasks;
using Label = SignalLabelingApp.Classes.Label;
using System.Collections.Generic;


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
                    BorderBrush = Brushes.Black, // Цвет рамки
                    BorderThickness = new Thickness(1), // Толщина рамки     
                };

                border.Child = labelBlock;

                var comboBox = new ComboBox
                {
                    SelectedIndex = 0,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // Добавляем кнопку удаления
                var deleteButton = new Button
                {
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Content = "Delete",
                };

                // Подписываемся на событие нажатия на кнопку удаления
                deleteButton.Click += (s, e) =>
                {
                    Globals.AllDatasetSamples.Remove(sample);
                };

                labelBlock.Children.Add(deleteButton);

                if (label is SignalClassificationLabel classificationLabel)
                {
                    var EventID = classificationLabel.EventID;
                    

                    comboBox.Items.Add(new TextBlock { Text = $"Type: Classification \n Event_ID {classificationLabel.EventID}\nStart: \nEnd {classificationLabel.ObjectStartPos}\nEnd: {classificationLabel.ObjectEndPos}\nClass: {classificationLabel.ObjectClass}" });
                    labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Classification" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {classificationLabel.ObjectStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {classificationLabel.ObjectEndPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Class: {classificationLabel.ObjectClass}" });

        

                }
                else if (label is SignalDetectionLabel detectionLabel)
                {
                    var EventID = detectionLabel.EventID;
                    comboBox.Items.Add(new TextBlock { Text = $"Type: Detection \n Event_ID: {detectionLabel.EventID}\nStart: {detectionLabel.SignalStartPos}\nEnd: {detectionLabel.SignalEndPos}" });
                    labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Detection" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {detectionLabel.SignalStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {detectionLabel.SignalEndPos}" });

                    foreach (var keyValuePair in detectionLabel.Objects)
                    {
                        var ObjID = keyValuePair.Key; 
                        DetectionObject obj = keyValuePair.Value;
                        comboBox.Items.Add(new TextBlock { Text =  $"  Objects:\nObjID={ObjID}\nObj_start={obj.X}\nObj_end={obj.W}\nObj_class={obj.Class}"} );

                        //labelBlock.Children.Add(new TextBlock { Text = $"Objects: Obj_start={obj.X}\n, Obj_end={obj.W}, \nObj_class={obj.Class}" });
                    }
                }
                else if (label is SignalSegmentationLabel segmentationLabel)
                {
                    var EventID = segmentationLabel.EventID;
                    comboBox.Items.Add(new TextBlock { Text = $"Type: Segmentation\n Event_ID: {segmentationLabel.EventID}\nStart: {segmentationLabel.ObjectStartPos}\nEnd: {segmentationLabel.ObjectEndPos}" });
                    labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Segmentation" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {segmentationLabel.ObjectStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {segmentationLabel.ObjectEndPos}" });
                }

                

                CreatedLabels.Children.Add(border);
                
            }
        }

    }
    
}