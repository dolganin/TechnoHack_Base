using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Interactivity;
using SignalLabelingApp.Classes;
using System.Threading.Tasks;
using Label = SignalLabelingApp.Classes.Label;
using System;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;


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
                    //BorderBrush = Brushes.Black, // Цвет рамки
                    //BorderThickness = new Thickness(1), // Толщина рамки
                };

                var borderGray = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(3),
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(3)
                };

                var borderGrayObj = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(3),
                    Margin = new Thickness(3),
                    CornerRadius = new CornerRadius(3)
                };

                border.Child = labelBlock;

                var comboBox = new ComboBox
                {
                    SelectedIndex = 0,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                var expanderBox = new Expander
                {
                    //Header = "id",
                    //Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch

                };

                var gridBox = new Grid(){
                    RowDefinitions = RowDefinitions.Parse("Auto Auto"),
                };

                expanderBox.Content = gridBox;

                // Добавляем кнопку удаления
                var deleteButton = new Button
                {
                    Margin = new Thickness(5),
                    //Height = 30,
                    
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Content = "X",
                };

                // Подписываемся на событие нажатия на кнопку удаления
                deleteButton.Click += (s, e) =>
                {
                    Globals.AllDatasetSamples.Remove(sample);
                };

                expanderBox.Content = gridBox;
                var expanderHeaderPanel = new StackPanel(){Orientation=Orientation.Horizontal};
                expanderHeaderPanel.Children.Add(deleteButton);
                expanderBox.Header = expanderHeaderPanel;

                labelBlock.Children.Add(expanderBox);

                if (label is SignalClassificationLabel classificationLabel)
                {
                    expanderHeaderPanel.Children.Add(new TextBlock(){Text=$"id:{classificationLabel.EventID} (cls)", VerticalAlignment=VerticalAlignment.Center});
                    
                    var stpTmp = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
                    Grid.SetRow(borderGray, 0);
                    borderGray.Child = stpTmp;
                    
                    stpTmp.Children.Add(new TextBlock { Text = $"Type: Classification\nEvent_id:{classificationLabel.EventID}\nStart: {Math.Round(classificationLabel.ObjectStartPos, 4)}\nEnd: {Math.Round(classificationLabel.ObjectEndPos, 4)}\nClass: {Math.Round(classificationLabel.ObjectClass, 4)}" });
                    gridBox.Children.Add(borderGray);

                    //comboBox.Items.Add(new TextBlock { Text = $"Type: Classification\nStart: {Math.Round(classificationLabel.ObjectStartPos, 4)}\nEnd: {Math.Round(classificationLabel.ObjectEndPos, 4)}\nClass: {Math.Round(classificationLabel.ObjectClass, 4)}" });
                    //labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Classification" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {classificationLabel.ObjectStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {classificationLabel.ObjectEndPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Class: {classificationLabel.ObjectClass}" });
                }
                else if (label is SignalDetectionLabel detectionLabel)
                {
                    expanderHeaderPanel.Children.Add(new TextBlock(){Text=$"id:{detectionLabel.EventID} (det)", VerticalAlignment=VerticalAlignment.Center});
                    
                    var stpTmp = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
                    Grid.SetRow(borderGray, 0);
                    borderGray.Child = stpTmp;
                    stpTmp.Children.Add(new TextBlock { Text = $"Type: Detection\nEvent_id:{detectionLabel.EventID}\nStart: {Math.Round(detectionLabel.SignalStartPos, 4)}\nEnd: {Math.Round(detectionLabel.SignalEndPos, 4)}" });
                    gridBox.Children.Add(borderGray);
                    
                    var stpTmpDetectionObj = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
                    Grid.SetRow(borderGrayObj, 1);
                    borderGrayObj.Child = stpTmpDetectionObj;
                    gridBox.Children.Add(borderGrayObj);

                    //comboBox.Items.Add(new TextBlock { Text = $"Type: Detection\nStart: {Math.Round(detectionLabel.SignalStartPos, 4)}\nEnd: {Math.Round(detectionLabel.SignalEndPos, 4)}" });
                    //labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Detection" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {detectionLabel.SignalStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {detectionLabel.SignalEndPos}" });

                    foreach (var pair in detectionLabel.Objects)
                    {
                        int objid = pair.Key;
                        DetectionObject obj = pair.Value;
                        stpTmpDetectionObj.Children.Add(new TextBlock { Text = $"Object:\nObj_id={objid}\nObj_start={obj.X}\nObj_len={obj.W}\nObj_class={obj.Class}" });
                        
                        //comboBox.Items.Add(new TextBlock { Text = $"Objects:\nObj_start={obj.X}\nObj_end={obj.W}\nObj_class={obj.Class}" });
                        //labelBlock.Children.Add(new TextBlock { Text = $"Objects: Obj_start={obj.X}\n, Obj_end={obj.W}, \nObj_class={obj.Class}" });
                    }
                }
                else if (label is SignalSegmentationLabel segmentationLabel)
                {
                    expanderHeaderPanel.Children.Add(new TextBlock(){Text=$"id:{segmentationLabel.EventID} (seg)", VerticalAlignment=VerticalAlignment.Center});

                    var stpTmp = new StackPanel { Orientation = Orientation.Vertical, Spacing = 5 };
                    Grid.SetRow(borderGray, 0);
                    borderGray.Child = stpTmp;
                    stpTmp.Children.Add(new TextBlock { Text = $"Type: Segmentation\nEvent_id:{segmentationLabel.EventID}\nStart: {Math.Round(segmentationLabel.ObjectStartPos, 4)}\nEnd: {Math.Round(segmentationLabel.ObjectEndPos, 4)}" });
                    gridBox.Children.Add(borderGray);
                    //comboBox.Items.Add(new TextBlock { Text = $"Type: Segmentation\nStart: {Math.Round(segmentationLabel.ObjectStartPos, 4)}\nEnd: {Math.Round(segmentationLabel.ObjectEndPos, 4)}" });
                    //labelBlock.Children.Add(comboBox);
                    //labelBlock.Children.Add(new TextBlock { Text = $"Type: Segmentation" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"Start: {segmentationLabel.ObjectStartPos}" });
                    //labelBlock.Children.Add(new TextBlock { Text = $"End: {segmentationLabel.ObjectEndPos}" });
                }



                CreatedLabels.Children.Add(border);

            }
        }

    }

}