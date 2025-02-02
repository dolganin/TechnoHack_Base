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
using SignalLabelingApp.ViewModels;

namespace SignalLabelingApp.Classes
{

    public class ObjectSelectionManager 
    {
        private NamedRectangle? singleSelectionRectangle; // Для ЛКМ выделения
        private Line? leftDashedLine;
        private Line? rightDashedLine;
        private List<TextBlock> blueTextBlocks = new(); // Для хранения TextBlock синих прямоугольников
        private List<TextBlock> orangeTextBlocks = new(); // Для хранения TextBlock оранжевых прямоугольников

        private bool isDrawingRectangle = false;
        private bool isRightClick = false; // Флаг для определения ПКМ
        private int startX = 0;
        public int blueRectangleIdCounter = 0; // Счетчик для ID синих прямоугольников
        public int orangeRectangleIdCounter = 0; // Счетчик для ID оранжевых прямоугольников

        public int objectClassId = 0;
        public string selectedLabelType = "Classification";
        public bool adaptiveSizeEnabled = false;
        public double adaptiveSizeValue = 10.0;
        public float DrawScaleX = 0.5f;


        private Canvas CanvasToTrack;

        public ObjectSelectionManager(Canvas canvasToTrack)
        {
            CanvasToTrack = canvasToTrack;
            CanvasToTrack.PointerPressed += Canvas_PointerPressed;
            CanvasToTrack.PointerMoved += Canvas_PointerMoved;
            CanvasToTrack.PointerReleased += Canvas_PointerReleased;
        }

        private void ShowError(string message)
        {
            var errorWindow = new ErrorWindow();
            var viewModel = errorWindow.DataContext as ErrorWindowViewModel;
            viewModel?.SetErrorMessage(message);
            errorWindow.Show();
        }

        public void SaveSelection()
        {
            if (singleSelectionRectangle == null)
            {
                ShowError("Error: No blue area selected using Left Mouse Button.");
                return;
            }

            // Check if orange zones are required
            if (selectedLabelType != "Classification" && singleSelectionRectangle.OrangeRectangles.Count == 0)
            {
                ShowError("Error: Orange zones are required for this task type. Please add at least one orange zone.");
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
                    ObjectClass = objectClassId,
                    EventID = singleSelectionRectangle.Id
                };
            }
            else if (selectedLabelType == "Detection")
            {
                var detectionLabel = new SignalDetectionLabel
                {
                    SignalStartPos = objectStartPos,
                    SignalEndPos = objectEndPos,
                    EventID = singleSelectionRectangle.Id
                };

                foreach (var rect in singleSelectionRectangle.OrangeRectangles)
                {
                    double rectStart = rect.Margin.Left / DrawScaleX;
                    double rectEnd = (rect.Margin.Left + rect.Width) / DrawScaleX;

                    if (rectStart >= objectStartPos && rectEnd <= objectEndPos)
                    {
                        //detectionLabel.Objects.Add(new Object {get;}, { X = (int)(rectStart - objectStartPos), W = (int)(rectEnd - rectStart), Class = objectClassId });
                        detectionLabel.Objects.Add( rect.Id, new DetectionObject() { X = (int)(rectStart - objectStartPos), W = (int)(rectEnd - rectStart), Class = objectClassId } );                 
                    }
                    else
                    {
                        ShowError("Error: One or more orange areas are outside the blue area.");
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
                    ObjectEndPos = objectEndPos,
                    EventID = singleSelectionRectangle.Id
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

            if (!isRightClick)
            {
                if (singleSelectionRectangle != null){
                // Удалить все старые области, если используется ЛКМ
                foreach (var old_rectangle in singleSelectionRectangle.OrangeRectangles)
                {
                    CanvasToTrack.Children.Remove(old_rectangle);
                    var txtBlock = orangeTextBlocks.FirstOrDefault(tb => tb.Text == old_rectangle.Name);
                    if (txtBlock != null)
                    {
                        CanvasToTrack.Children.Remove(txtBlock);
                    }
                }
                singleSelectionRectangle.OrangeRectangles.Clear();
                orangeTextBlocks.Clear();
                
                }
                if (singleSelectionRectangle != null)
                {
                    CanvasToTrack.Children.Remove(singleSelectionRectangle);
                    var txtBlock = blueTextBlocks.FirstOrDefault(tb => tb.Text == singleSelectionRectangle.Name);
                    if (txtBlock != null)
                    {
                        CanvasToTrack.Children.Remove(txtBlock);
                    }
                    singleSelectionRectangle = null;
                }
                blueTextBlocks.Clear();
            }

            if (isRightClick && singleSelectionRectangle == null)
            {
                ShowError("Error: No blue area selected. Please select a blue area first.");
                isDrawingRectangle = false;
                return;
            }

            var fillColor = isRightClick ? Colors.Orange : Colors.LightBlue;
            var rectangle = new NamedRectangle(0)
            {
                Fill = new ImmutableSolidColorBrush(fillColor, 0.5),
                Height = CanvasToTrack.Bounds.Height,
                Width = 0,
                Margin = new Thickness(startX, 0, 0, 0)
            };

                CanvasToTrack.Children.Add(rectangle);

                if (isRightClick)
                {
                    singleSelectionRectangle.OrangeRectangles.Add(rectangle);
                }
                else
                {
                    singleSelectionRectangle = rectangle;
                }
            
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (!isDrawingRectangle)
                return;

            var position = e.GetPosition(CanvasToTrack);
            double currentX = position.X;
            double rectWidth = Math.Abs(currentX - startX);

            var rectangle = isRightClick ? singleSelectionRectangle.OrangeRectangles.Last() : singleSelectionRectangle;

            if (rectangle != null)
            {
                if (isRightClick && singleSelectionRectangle != null)
                {
                    double blueLeft = singleSelectionRectangle.Margin.Left;
                    double blueRight = blueLeft + singleSelectionRectangle.Width;

                    if (currentX < startX)
                    {
                        // Moving left
                        if (currentX < blueLeft)
                        {
                            currentX = blueLeft;
                        }
                        rectangle.Margin = new Thickness(currentX, 0, 0, 0);
                        rectangle.Width = startX - currentX;
                    }
                    else
                    {
                        // Moving right
                        if (currentX > blueRight)
                        {
                            currentX = blueRight;
                        }
                        rectangle.Margin = new Thickness(startX, 0, 0, 0);
                        rectangle.Width = currentX - startX;
                    }
                }
                else
                {
                    rectangle.Width = rectWidth;

                    if (currentX < startX)
                        rectangle.Margin = new Thickness(currentX, 0, 0, 0);
                    else
                        rectangle.Margin = new Thickness(startX, 0, 0, 0);
                }
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!isDrawingRectangle)
                return;

            isDrawingRectangle = false;

            var rectangle = isRightClick ? singleSelectionRectangle.OrangeRectangles.Last() : singleSelectionRectangle;

            if (rectangle != null)
            {
                double minWidth = adaptiveSizeEnabled ? 0 : isRightClick ? 10 : 50;

                if (rectangle.Width >= minWidth)
                {
                    rectangle.SetId(isRightClick ? ++orangeRectangleIdCounter : ++blueRectangleIdCounter);

                    var textBlock = new TextBlock
                    {
                        Text = rectangle.Name,
                        Foreground = Brushes.Black,
                        FontSize = isRightClick ? 20 : 30, // Устанавливаем размер шрифта в зависимости от типа прямоугольника
                        FontWeight = FontWeight.Bold,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };

                    CanvasToTrack.Children.Add(textBlock);
                    Canvas.SetLeft(textBlock, rectangle.Margin.Left + rectangle.Width / 2);
                    Canvas.SetTop(textBlock, rectangle.Height / 2);

                    if (isRightClick)
                    {
                        orangeTextBlocks.Add(textBlock);
                    }
                    else
                    {
                        blueTextBlocks.Add(textBlock);
                    }

                    if (isRightClick && singleSelectionRectangle != null)
                    {
                        var orangeRectangle = singleSelectionRectangle.OrangeRectangles.Last();

                        if (orangeRectangle.Margin.Left < singleSelectionRectangle.Margin.Left ||
                            orangeRectangle.Margin.Left + orangeRectangle.Width > singleSelectionRectangle.Margin.Left + singleSelectionRectangle.Width)
                        {
                            ShowError("Error: The orange area is outside the bounds of the blue area.");
                            CanvasToTrack.Children.Remove(orangeRectangle);
                            var txtBlock = orangeTextBlocks.Last();
                            CanvasToTrack.Children.Remove(txtBlock);
                            singleSelectionRectangle.OrangeRectangles.Remove(orangeRectangle);
                            orangeTextBlocks.Remove(txtBlock);
                        }
                        //else
                        //{
                        //    singleSelectionRectangle.OrangeRectangles.Add(orangeRectangle);
                        //}
                    }

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

                        var finalTextBlock = blueTextBlocks.Last();
                        if (finalTextBlock != null)
                        {
                            Canvas.SetLeft(finalTextBlock, singleSelectionRectangle.Margin.Left + singleSelectionRectangle.Width / 2);
                            Canvas.SetTop(finalTextBlock, singleSelectionRectangle.Height / 2);
                        }
                    }

                    AddDashedLines(rectangle);
                }
                else
                {
                    // Удаляем прямоугольник, если его ширина меньше минимальной
                    CanvasToTrack.Children.Remove(rectangle);
                }
            }
        }

        private void AddDashedLines(NamedRectangle rectangle)
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