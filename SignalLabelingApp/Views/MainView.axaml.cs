using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
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

        private void UpdateDatasetSamplesView()
        {
            CreatedLabels.Children.Clear();

            foreach (var sample in Globals.AllDatasetSamples)
            {
                Label label = sample.Label;
                var labelBlock = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5)
                };

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

               CreatedLabels.Children.Add(labelBlock);
            }
        }

    }
}