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
using Avalonia.Data.Converters;
using System.Globalization;


namespace SignalLabelingApp.Views
{
    public partial class OyControl: UserControl 
    {
        public TextBlock maxValue { get; set; }
        public TextBlock middleValue { get;  set; }
        public TextBlock minValue { get;  set; }

        public OyControl()
        {
            InitializeComponent();
            maxValue = MaxValue;
            middleValue = MiddleValue;
            minValue = MinValue;
            ArrowCanvas.SizeChanged += ArrowCanvas_SizeChanged;
            UpdateArrowGeometry(); // Initial update
        }

    

        private void ArrowCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                UpdateArrowGeometry();
            }

        private void UpdateArrowGeometry()
        {
            double canvasWidth = ArrowCanvas.Bounds.Width;
            double canvasHeight = ArrowCanvas.Bounds.Height;
            double arrowWidth = canvasWidth * 0.1; // 10% of canvas width
            double arrowHeight = canvasHeight;

            var geometryGroup = new GeometryGroup
            {
                Children = new GeometryCollection
                {
                    // Вертикальная стрелка
                    new LineGeometry(new Point(canvasWidth / 2, 0), new Point(canvasWidth / 2, arrowHeight)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.1), new Point(canvasWidth / 2, 0)),
                    new LineGeometry(new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.1), new Point(canvasWidth / 2, 0)),

                    // Узкие деления
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.05), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.05)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.5), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.5)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.95), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.95))
                }
            };

            ArrowPath.Data = geometryGroup;
        }
    }
    public class CanvasSizeToLineGeometryConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count < 2 || !(values[0] is double canvasWidth) || !(values[1] is double canvasHeight))
                return null;

            double arrowWidth = canvasWidth * 0.1; // 10% of canvas width
            double arrowHeight = canvasHeight;

            var geometryGroup = new GeometryGroup
            {
                Children = new GeometryCollection
                {
                    // Вертикальная стрелка
                    new LineGeometry(new Point(canvasWidth / 2, 0), new Point(canvasWidth / 2, arrowHeight)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.1), new Point(canvasWidth / 2, 0)),
                    new LineGeometry(new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.1), new Point(canvasWidth / 2, 0)),

                    // Узкие деления
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.2), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.2)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.5), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.5)),
                    new LineGeometry(new Point(canvasWidth / 2 - arrowWidth / 2, arrowHeight * 0.8), new Point(canvasWidth / 2 + arrowWidth / 2, arrowHeight * 0.8))
                }
            };

            return geometryGroup;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}