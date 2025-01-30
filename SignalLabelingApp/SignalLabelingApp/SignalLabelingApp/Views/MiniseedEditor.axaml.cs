using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SignalLabelingApp.Classes;
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
        public ScrollViewer EditorSñrollViewer;
        public Slider ScaleXSlider;

        private Flyout settingsFlyout;

        private ObjectSelectionManager objectSelectionManager;

        public StationData currentStationData;

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
            Grid EditorGrid = new Grid
            {
                IsHitTestVisible = true,
            };

            var canvasScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            };
            EditorSñrollViewer = canvasScrollViewer;

            var canvas = new Canvas
            {
                Background = Brushes.LightGray
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
                Maximum = 2.0,
                Value = objectSelectionManager.DrawScaleX,
                Width = 200,
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
            EditorSñrollViewer.Content = EditorCanvas;

            EditorGrid.Children.Add(EditorSñrollViewer);
            EditorGrid.Children.Add(buttonPanel);
            EditorGrid.Children.Add(stationComboBox);
            EditorGrid.Children.Add(scaleXSlider);

            EditorBorder.Child = EditorGrid;
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
                // Ïðîâåðÿåì, ÷òî ââîäèìîå çíà÷åíèå — öèôðà
                if (!int.TryParse(e.Text, out _))
                {
                    e.Handled = true; // Çàïðåùàåì ââîä, åñëè ýòî íå ÷èñëî
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
                double previousOffsetX = EditorSñrollViewer.Offset.X;
                objectSelectionManager.DrawScaleX = (float)newValue;

                UpdateCanvasWidth();

                double newOffsetX = previousOffsetX * (newValue / oldValue);
                EditorSñrollViewer.Offset = new Vector(newOffsetX, EditorSñrollViewer.Offset.Y);

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
                
                EditorSñrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RedrawVisibleSignal();
            //objectSelectionManager.RedrawObjectSelections();
        }

        private void RedrawVisibleSignal()
        {
            if (currentStationData == null) return;

            EditorCanvas.Children.Clear();

            var stationComboBox = new ComboBox
            {
                Margin = new Thickness(10, 10, 10, 0)
            };

            EditorCanvas.Children.Add(stationComboBox);

            double startX = EditorSñrollViewer.Offset.X;
            double endX = startX + EditorSñrollViewer.Viewport.Width;

            int channelHeight = (int)(EditorCanvas.Bounds.Height / 3);

            RedrawOneChannel(0, channelHeight, currentStationData.Channel1, startX, endX, (ImmutableSolidColorBrush)Brushes.Blue);
            RedrawOneChannel(channelHeight, channelHeight, currentStationData.Channel2, startX, endX, (ImmutableSolidColorBrush)Brushes.Green);
            RedrawOneChannel(2 * channelHeight, channelHeight, currentStationData.Channel3, startX, endX, (ImmutableSolidColorBrush)Brushes.Red);
        }

        private void RedrawOneChannel(int channelStartY, int channelHeight, TraceData trace, double startX, double endX, ImmutableSolidColorBrush brush)
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
