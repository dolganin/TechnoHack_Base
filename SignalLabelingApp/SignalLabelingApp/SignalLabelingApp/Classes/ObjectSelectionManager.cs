﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media.Immutable;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using SignalLabelingApp.Views;

namespace SignalLabelingApp.Classes
{
    public class SelectionObject
    {
        public string LabelType { get; set; } // Тип лейбла (Classification, Detection, etc.)
        public double StartPosition { get; set; } // Начальная позиция
        public double EndPosition { get; set; } // Конечная позиция
        public ImmutableSolidColorBrush MainColor { get; set; } // Цвет основного выделения
        public List<(double Start, double End, ImmutableSolidColorBrush Color)> AdditionalSelections { get; set; } // Дополнительные выделения (например, оранжевые области)

    }


    public class ObjectSelectionManager
    {
        private Rectangle? singleSelectionRectangle; // Для ЛКМ выделения
        private Line? leftDashedLine;
        private Line? rightDashedLine;
        private List<Rectangle> multiSelectionRectangles = new(); // Для ПКМ выделений


        private bool isDrawingRectangle = false;
        private bool isRightClick = false; // Флаг для определения ПКМ
        private int startX = 0;

        public int objectClassId = 0;
        public string selectedLabelType = "Classification";
        public bool adaptiveSizeEnabled = false;
        public double adaptiveSizeValue = 10.0;
        public float DrawScaleX = 0.5f;

        private Rectangle? currentRectangle;
        private Line? startDashedLine;

        private Canvas CanvasToTrack;

        public ObjectSelectionManager(Canvas canvasToTrack) {

            CanvasToTrack = canvasToTrack;
            CanvasToTrack.PointerPressed += Canvas_PointerPressed;
            CanvasToTrack.PointerMoved += Canvas_PointerMoved;
            CanvasToTrack.PointerReleased += Canvas_PointerReleased;
        }



        public void SaveSelection()
        {
            if (singleSelectionRectangle == null)
            {
                Console.WriteLine("Error: No blue area selected using Left Mouse Button.");
                return;
            }

            double objectStartPos = singleSelectionRectangle.Margin.Left / DrawScaleX;
            double objectEndPos = (singleSelectionRectangle.Margin.Left + singleSelectionRectangle.Width) / DrawScaleX;

            
            Label label = null;

            if (selectedLabelType == "Classification")
            {
                label = new SignalClassificationLabel
                {
                    ObjectStartPos = objectStartPos,
                    ObjectEndPos = objectEndPos,
                    ObjectClass = objectClassId
                };
            }
            else if (selectedLabelType == "Detection")
            {
                var detectionLabel = new SignalDetectionLabel
                {
                    SignalStartPos = objectStartPos,
                    SignalEndPos = objectEndPos,
                    Objects = new ObservableCollection<DetectionObject>()
                };

                foreach (var rect in multiSelectionRectangles)
                {
                    double rectStart = rect.Margin.Left / DrawScaleX;
                    double rectEnd = (rect.Margin.Left + rect.Width) / DrawScaleX;

                    Console.WriteLine($"Checking orange area: Start={rectStart}, End={rectEnd}");
                    Console.WriteLine($"Blue area bounds: Start={objectStartPos}, End={objectEndPos}");

                    if (rectStart >= objectStartPos && rectEnd <= objectEndPos)
                    {
                        detectionLabel.Objects.Add(new DetectionObject() { X = (int)(rectStart - objectStartPos), W = (int)(rectEnd - rectStart), Class = objectClassId });
                    }
                    else
                    {
                        Console.WriteLine("Error: One or more orange areas are outside the blue area.");
                        return;
                    }
                }

                label = detectionLabel;
            }
            else if (selectedLabelType == "Segmentation")
            {
                label = new SignalSegmentationLabel
                {
                    ObjectStartPos = objectStartPos,
                    ObjectEndPos = objectEndPos
                };
            }

            if (label != null)
            {
                TraceFragment fragment = GetTraceFragment(objectStartPos, objectEndPos);
                if (fragment is not null)
                {
                    DatasetSample datasetSample = new DatasetSample()
                    {
                        Label = label,
                        TraceFragment = fragment,
                    };

                    AddDatasetSampleToGlobals(datasetSample);
                }
            }
        }

        private TraceFragment GetTraceFragment(double startX, double endX)
        {
            TraceFragment traceFragment = null;

            if (Globals.CurrentEditor is MiniseedEditor miniseedEditor)
            {
                StationData stationData = miniseedEditor.currentStationData;

                if (stationData != null)
                {
                    // Преобразуем индексы startX и endX в целочисленные значения
                    int startIndex = (int)Math.Floor(startX);
                    int endIndex = (int)Math.Ceiling(endX);

                    // Создаем новый объект TraceFragment
                    traceFragment = new TraceFragment
                    {
                        Channel1 = ExtractDataByIndices(stationData.Channel1, startIndex, endIndex),
                        Channel2 = ExtractDataByIndices(stationData.Channel2, startIndex, endIndex),
                        Channel3 = ExtractDataByIndices(stationData.Channel3, startIndex, endIndex)
                    };
                }
            }

            return traceFragment;
        }

        private List<float> ExtractDataByIndices(TraceData traceData, int startIndex, int endIndex)
        {
            if (traceData == null || traceData.data == null || traceData.data.Count == 0)
            {
                return new List<float>();
            }

            // Ограничиваем индексы в допустимых пределах
            startIndex = Math.Max(0, startIndex);
            endIndex = Math.Min(traceData.data.Count - 1, endIndex);

            // Если диапазон некорректен, возвращаем пустой список
            if (startIndex > endIndex)
            {
                return new List<float>();
            }

            // Извлекаем данные в заданном диапазоне
            return traceData.data.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
        }


        private void AddDatasetSampleToGlobals(DatasetSample sample)
        {
            if (!Globals.AllDatasetSamples.Contains(sample))
            {
                Globals.AllDatasetSamples.Add(sample);
            }
        }

        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var position = e.GetPosition(CanvasToTrack);
            startX = (int)position.X;
            isDrawingRectangle = true;
            isRightClick = e.GetCurrentPoint(CanvasToTrack).Properties.IsRightButtonPressed;

            if (!isRightClick && singleSelectionRectangle != null)
            {
                // Удалить старую область, если используется ЛКМ
                CanvasToTrack.Children.Remove(singleSelectionRectangle);
                singleSelectionRectangle = null;
            }

            var fillColor = isRightClick ? Colors.Orange : Colors.LightBlue;
            var rectangle = new Rectangle
            {
                Fill = new ImmutableSolidColorBrush(fillColor, 0.5),
                Height = CanvasToTrack.Bounds.Height,
                Width = 0,
                Margin = new Thickness(startX, 0, 0, 0)
            };

            CanvasToTrack.Children.Add(rectangle);

            if (isRightClick)
                multiSelectionRectangles.Add(rectangle);
            else
                singleSelectionRectangle = rectangle;
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (!isDrawingRectangle)
                return;

            var position = e.GetPosition(CanvasToTrack);
            double currentX = position.X;
            double rectWidth = Math.Abs(currentX - startX);

            var rectangle = isRightClick ? multiSelectionRectangles.Last() : singleSelectionRectangle;

            if (rectangle != null)
            {
                rectangle.Width = rectWidth;

                if (currentX < startX)
                    rectangle.Margin = new Thickness(currentX, 0, 0, 0);
                else
                    rectangle.Margin = new Thickness(startX, 0, 0, 0);
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            isDrawingRectangle = false;

            if (!isRightClick && adaptiveSizeEnabled && singleSelectionRectangle != null)
            {
                // Применение адаптивного размера только для ЛКМ
                double adjustedWidth = adaptiveSizeValue * DrawScaleX;

                singleSelectionRectangle.Width = adjustedWidth;

                // Проверяем выход за пределы Canvas
                if (singleSelectionRectangle.Margin.Left + singleSelectionRectangle.Width > CanvasToTrack.Width)
                {
                    singleSelectionRectangle.Width = CanvasToTrack.Width - singleSelectionRectangle.Margin.Left;
                }
            }

            // Добавляем пунктирные линии на границы области
            if (!isRightClick && singleSelectionRectangle != null)
            {
                AddDashedLines(singleSelectionRectangle);
            }

            currentRectangle = null;
            startDashedLine = null;
        }

        private void AddDashedLines(Rectangle rectangle)
        {
            // Удаляем старые линии, если они существуют
            if (leftDashedLine != null) CanvasToTrack.Children.Remove(leftDashedLine);
            if (rightDashedLine != null) CanvasToTrack.Children.Remove(rightDashedLine);

            double leftX = rectangle.Margin.Left;
            double rightX = leftX + rectangle.Width;

            leftDashedLine = new Line
            {
                StartPoint = new Avalonia.Point(leftX, 0),
                EndPoint = new Avalonia.Point(leftX, CanvasToTrack.Bounds.Height),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                StrokeDashArray = new AvaloniaList<double> { 4, 2 }
            };

            rightDashedLine = new Line
            {
                StartPoint = new Avalonia.Point(rightX, 0),
                EndPoint = new Avalonia.Point(rightX, CanvasToTrack.Bounds.Height),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                StrokeDashArray = new AvaloniaList<double> { 4, 2 }
            };

            CanvasToTrack.Children.Add(leftDashedLine);
            CanvasToTrack.Children.Add(rightDashedLine);
        }

    }
}
