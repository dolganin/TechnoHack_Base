using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SignalLabelingApp.Classes;
// using SignalLabelingApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Media.Immutable;
using Avalonia.Collections;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using Label = SignalLabelingApp.Classes.Label;

namespace SignalLabelingApp.Views
{
    public partial class MiniseedEditor : EditorBase
    {
        public Canvas EditorCanvas;
        public ScrollViewer EditorScrollViewer;
        public Slider ScaleXSlider;

        private Flyout settingsFlyout;

        private ObjectSelectionManager objectSelectionManager;

        public StationData currentStationData;

        public OyControl Ch1Oy = null;
        public OyControl Ch2Oy = null;
        public OyControl Ch3Oy = null;

        public OxControl Ox = null; 

        //public Grid mainGrid;

        public MiniseedEditor()
        {
            InitializeComponent();
        }


        public override void LoadFromFile(string filePath)
        {
            MiniseedFile miniseedFile = PythonMiniseedReader.ReadMiniseedFile(filePath);
            DrawSignalsFromMiniseed(miniseedFile);
        }

        public void DrawSignalsFromMiniseed(MiniseedFile miniseedFile)
        {

            Grid GeneralGrid = new Grid
            {
                IsHitTestVisible = true,
                ColumnDefinitions = new ColumnDefinitions("Auto *"),
                RowDefinitions = new RowDefinitions("* 40"),
            Background = new SolidColorBrush(Color.Parse("#EEE"))

            };

            Grid EditorGrid = new Grid{};

            //mainGrid = EditorGrid;

            var oyControlGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,*,*") // Три строки для трех OyControl
            };

            // Создаем три экземпляра OyControl
            var oyControl1 = new OyControl(){
                Width = 80 
            };
            var oyControl2 = new OyControl()
            {
                Width = 80
            };
            var oyControl3 = new OyControl()
            {
                Width = 80
            };

            Ch1Oy = oyControl1;
            Ch2Oy = oyControl2;
            Ch3Oy = oyControl3;
            

            var oxControl = new OxControl()
            {
            
            };

            Ox = oxControl;

            // Добавляем OyControl в соответствующие строки
            Grid.SetRow(oyControl1, 0);
            Grid.SetRow(oyControl2, 1);
            Grid.SetRow(oyControl3, 2);

            oyControlGrid.Children.Add(oyControl1);
            oyControlGrid.Children.Add(oyControl2);
            oyControlGrid.Children.Add(oyControl3);

            var canvasScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            };

            EditorScrollViewer = canvasScrollViewer;
            var canvas = new Canvas(){
                HorizontalAlignment= HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.Transparent
            };

            EditorCanvas = canvas;
            objectSelectionManager = new ObjectSelectionManager(EditorCanvas);

            var stationComboBox = new ComboBox();
            
            foreach (StationData stationData in miniseedFile.stationDataStructures)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = stationData.StationName,
                    Tag = stationData
                };

                item.PointerPressed += StationItem_PointerPressed;
                stationComboBox.Items.Add(item);
            }

            var scaleXSlider = new Slider
            {
                Minimum = 0.05,
                Maximum = 3.0,
                Value = objectSelectionManager.DrawScaleX,
                Width = 300,
                Height = 50,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Thickness(10, 10, 10, 10)
            };


            scaleXSlider.PropertyChanged += ScaleXSlider_ValueChanged;
            ScaleXSlider = scaleXSlider;

            settingsFlyout = new Flyout
            {
                Content = CreateSettingsFlyoutContent()
            };

            var settingsButton = new Button
            {
                Content = "*",
                Width = 30,
                Height = 30,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 10, 0)
            };
            settingsButton.Flyout = settingsFlyout;

            var saveButton = new Button
            {
                Content = "Save",
                Width = 70,
                Height = 30,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 50, 0)
            };

            saveButton.Click += SaveButton_Click;

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 10, 0),
                Spacing = 10
            };
            
            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(settingsButton);

            stationComboBox.Margin = new Thickness(10, 10, 10, 10);
            EditorScrollViewer.Content = EditorCanvas;

            Grid.SetColumn(oyControlGrid, 0);
            Grid.SetRow(oyControlGrid, 0);
            GeneralGrid.Children.Add(oyControlGrid);

            Grid.SetColumn(oxControl, 1);
            Grid.SetRow(oxControl, 1);
            GeneralGrid.Children.Add(oxControl);

            EditorGrid.Children.Add(EditorScrollViewer);

            EditorGrid.Children.Add(buttonPanel);
            EditorGrid.Children.Add(stationComboBox);
            EditorGrid.Children.Add(scaleXSlider);

            Grid.SetColumn(EditorGrid, 1);
            Grid.SetRow(EditorGrid, 0);
            GeneralGrid.Children.Add(EditorGrid);

            EditorBorder.Child = GeneralGrid;
        }

        private Control CreateSettingsFlyoutContent()
        {
            var stackPanel = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(10)
            };

            var objectClassTextBox = new TextBox
            {
                Watermark = "Enter ObjectClassID"
            };
            objectClassTextBox.PropertyChanged += (sender, e) =>
            {
                if (e.Property == TextBox.TextProperty && int.TryParse(objectClassTextBox.Text, out var result))
                {
                    objectSelectionManager.objectClassId = result;
                }
            };
            stackPanel.Children.Add(objectClassTextBox);

            var labelTypeComboBox = new ComboBox
            {
                ItemsSource = new List<string> { "Classification", "Detection", "Segmentation" },
                SelectedIndex = 0
            };
            labelTypeComboBox.SelectionChanged += (sender, e) =>
            {
                if (labelTypeComboBox.SelectedItem is string selectedType)
                {
                    objectSelectionManager.selectedLabelType = selectedType;
                }
            };
            stackPanel.Children.Add(labelTypeComboBox);

            var adaptiveSizeToggle = new ToggleSwitch
            {
                Content = "Enable Adaptive Size"
            };
            adaptiveSizeToggle.IsCheckedChanged += (sender, e) =>
            {
                objectSelectionManager.adaptiveSizeEnabled = adaptiveSizeToggle.IsChecked ?? false;
            };
            stackPanel.Children.Add(adaptiveSizeToggle);

            var adaptiveSizeTextBox = new TextBox
            {
                Watermark = "Adaptive Size"
            };
            adaptiveSizeTextBox.AddHandler(TextInputEvent, (sender, e) =>
            {
                // ���������, ��� �������� �������� � �����
                if (!int.TryParse(e.Text, out _))
                {
                    e.Handled = true; // ��������� ����, ���� ��� �� �����
                }
            }, RoutingStrategies.Tunnel);
            adaptiveSizeTextBox.PropertyChanged += (sender, e) =>
            {
                if (e.Property == TextBox.TextProperty && double.TryParse(adaptiveSizeTextBox.Text, out var result))
                {
                    objectSelectionManager.adaptiveSizeValue = result;
                }
            };
            stackPanel.Children.Add(adaptiveSizeTextBox);

            return stackPanel;
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            objectSelectionManager.SaveSelection();
        }

        private void ScaleXSlider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Slider.ValueProperty && e.NewValue is double newValue && e.OldValue is double oldValue)
            {
                double previousOffsetX = EditorScrollViewer.Offset.X;
                objectSelectionManager.DrawScaleX = (float)newValue;

                UpdateCanvasWidth();

                double newOffsetX = previousOffsetX * (newValue / oldValue);
                EditorScrollViewer.Offset = new Vector(newOffsetX, EditorScrollViewer.Offset.Y);

                // UpdateOyControlValues();

            }
        }

        private void UpdateCanvasWidth()
        {
            if (currentStationData == null || EditorCanvas == null)
                return;

            int maxSamples = new[] {
                currentStationData.Channel1?.data?.Count ?? 0,
                currentStationData.Channel2?.data?.Count ?? 0,
                currentStationData.Channel3?.data?.Count ?? 0
            }.Max();

            EditorCanvas.Width = maxSamples * objectSelectionManager.DrawScaleX;
        }
        
        private void StationItem_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is ComboBoxItem item && item.Tag is StationData stationData)
            {
                currentStationData = stationData;

                int maxSamples = new[] {
                    stationData.Channel1?.data?.Count ?? 0,
                    stationData.Channel2?.data?.Count ?? 0,
                    stationData.Channel3?.data?.Count ?? 0
                }.Max();

                EditorCanvas.Width = maxSamples * objectSelectionManager.DrawScaleX;

                RedrawVisibleSignal();
                
                EditorScrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RedrawVisibleSignal();
            // UpdateOyControlValues();
            //objectSelectionManager.RedrawObjectSelections();
        }

        private void RedrawVisibleSignal()
        {
            if (currentStationData == null) return;

            EditorCanvas.Children.Clear();

            double startX = EditorScrollViewer.Offset.X;
            double endX = startX + EditorScrollViewer.Viewport.Width;

            int channelHeight = (int)(EditorCanvas.Bounds.Height / 3);

            RedrawOneChannel(Ch1Oy, 0, channelHeight, currentStationData.Channel1, startX, endX, (ImmutableSolidColorBrush)Brushes.Blue);
            RedrawOneChannel(Ch2Oy, channelHeight, channelHeight, currentStationData.Channel2, startX, endX, (ImmutableSolidColorBrush)Brushes.Green);
            RedrawOneChannel(Ch3Oy, 2 * channelHeight, channelHeight, currentStationData.Channel3, startX, endX, (ImmutableSolidColorBrush)Brushes.Red);
            RedrawOx(Ox, currentStationData.Channel1, startX, endX);


        }

        private void RedrawOx(OxControl currentOxControl, TraceData trace, double startX, double endX)
        {

            int startIndex = Math.Max(0, (int)(startX / objectSelectionManager.DrawScaleX));
            int endIndex = Math.Min(trace.data.Count, (int)(endX / objectSelectionManager.DrawScaleX));

            float minIndex = startIndex;
            float Index1 = (float)((endIndex - startIndex) * 0.1 + startIndex);
            float Index2 = (float)((endIndex - startIndex) * 0.2 + startIndex);
            float Index3 = (float)((endIndex - startIndex) * 0.3 + startIndex);
            float Index4 = (float)((endIndex - startIndex) * 0.4 + startIndex);
            float middleIndex = (float)((endIndex - startIndex) * 0.5 + startIndex);
            float Index6 = (float)((endIndex - startIndex) * 0.6 + startIndex);
            float Index7 = (float)((endIndex - startIndex) * 0.7 + startIndex);
            float Index8 = (float)((endIndex - startIndex) * 0.8 + startIndex);
            float Index9 = (float)((endIndex - startIndex) * 0.9 + startIndex);
            float maxIndex = (float)(endIndex);

            if (maxIndex == 0)
                return;

            if (currentOxControl != null)
            {

                currentOxControl.MinIndex.Text = minIndex.ToString();
                currentOxControl.Index1.Text = Index1.ToString();
                currentOxControl.Index2.Text = Index2.ToString();
                currentOxControl.Index3.Text = Index3.ToString();
                currentOxControl.Index4.Text = Index4.ToString();
                currentOxControl.MiddleIndex.Text = middleIndex.ToString();
                currentOxControl.Index6.Text = Index6.ToString();
                currentOxControl.Index7.Text = Index7.ToString();
                currentOxControl.Index8.Text = Index8.ToString();
                currentOxControl.Index9.Text = Index9.ToString();
                currentOxControl.MaxIndex.Text = maxIndex.ToString();
            }

        }

        private void RedrawOneChannel(OyControl currentOyControl, int channelStartY, int channelHeight, TraceData trace, double startX, double endX, ImmutableSolidColorBrush brush)
        {
            if (trace == null || trace.data == null || trace.data.Count == 0)
                return;

            var polyline = new Polyline
            {
                Stroke = brush,
                StrokeThickness = 1
            };
            var HorPolyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            int startIndex = Math.Max(0, (int)(startX / objectSelectionManager.DrawScaleX));
            int endIndex = Math.Min(trace.data.Count, (int)(endX / objectSelectionManager.DrawScaleX));

            float maxValue = GetMaxInRange(trace, startIndex, endIndex); 
            float minValue = GetMinInRange(trace, startIndex, endIndex);
            float middleValue = (maxValue + minValue)/2;

            

            if (maxValue == 0)
                return;

            float centerY = channelStartY + channelHeight / 2;

            for (int i = startIndex; i < endIndex; i++)
            {
                float x = i * objectSelectionManager.DrawScaleX;
                float y = centerY - (2 * (float)(trace.data[i] - minValue) / (maxValue - minValue) - 1) * ((float)(channelHeight) / 2 - 2); 

                polyline.Points.Add(new Avalonia.Point(x, y));
                HorPolyline.Points.Add(new Avalonia.Point(x, channelStartY + channelHeight));
            }

            EditorCanvas.Children.Add(polyline);
            EditorCanvas.Children.Add(HorPolyline);

            if (currentOyControl != null)
            {
                currentOyControl.MaxValue.Text = maxValue.ToString();
                currentOyControl.MiddleValue.Text = middleValue.ToString();
                currentOyControl.MinValue.Text = minValue.ToString();
            }

        }

        public static float GetMaxInRange(TraceData trace, int x1, int x2)
        {
            if (trace == null || trace.data == null || trace.data.Count == 0)
                throw new ArgumentException("Trace data is null or empty.");

            x1 = Math.Max(0, x1);
            x2 = Math.Min(trace.data.Count, x2);

            if (x1 >= x2)
                throw new ArgumentException("Invalid range: x1 should be less than x2.");

            return trace.data.Skip(x1).Take(x2 - x1).Max();
        }

        public static float GetMinInRange(TraceData trace, int x1, int x2)
        {
            if (trace == null || trace.data == null || trace.data.Count == 0)
                throw new ArgumentException("Trace data is null or empty.");

            x1 = Math.Max(0, x1);
            x2 = Math.Min(trace.data.Count, x2);

            if (x1 >= x2)
                throw new ArgumentException("Invalid range: x1 should be less than x2.");

            return trace.data.Skip(x1).Take(x2 - x1).Min();
        }


    }
}
