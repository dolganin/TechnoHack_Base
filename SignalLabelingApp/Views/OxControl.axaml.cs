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
    public partial class OxControl : UserControl
    {
        public TextBlock MaxValue { get; set; }
        public TextBlock Value1 { get; set; }
        public TextBlock Value2 { get; set; }
        public TextBlock Value3 { get; set; }
        public TextBlock Value4 { get; set; }
        public TextBlock MiddleValue { get; set; }
        public TextBlock Value6 { get; set; }
        public TextBlock Value7 { get; set; }
        public TextBlock Value8 { get; set; }
        public TextBlock Value9 { get; set; }

        public TextBlock MinValue { get; set; }

        public OxControl()
        {
            InitializeComponent();
            maxValue = MaxValue;
            middleValue = MiddleValue;
            minValue = MinValue;
            ArrowOxCanvas.SizeChanged += ArrowOxCanvas_SizeChanged;
            UpdateArrowOxGeometry(); // Initial update
        }



        private void ArrowOxCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateArrowOxGeometry();
        }

        private void UpdateArrowOxGeometry()
        {
            double canvasWidth = ArrowOxCanvas.Bounds.Width;
            double canvasHeight = ArrowOxCanvas.Bounds.Height;
            double arrowOxWidth = canvasWidth;
            double arrowOxHeight = canvasHeight * 0.1;

            var geometryGroupOx = new GeometryGroup
            {
                Children = new GeometryCollection
                {
                    // Вертикальная стрелка
                    new LineGeometry(new Point(0, canvasHeight / 2), new Point(canvasWidth, canvasHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.05, canvasHeight / 2 - arrowOxHeight / 2 ), new Point(canvasHeight / 2, 0)),
                    new LineGeometry(new Point(arrowOxWidth * 0.05, canvasHeight / 2 + arrowOxHeight / 2 ), new Point(canvasHeight / 2, 0)),

                    // Узкие деления
                    new LineGeometry(new Point(arrowOxWidth * 0.05, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.05, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.1, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.1, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.2, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.2, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.3, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.3, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.4, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.4, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.5, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.5, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.6, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.6, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.7, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.7, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.8, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.8, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.9, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.9, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.95, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.95, canvasHeight / 2 + arrowOxHeight / 2)),
                    
                }
            };

            ArrowOxPath.Data = geometryGroupOx;
        }
    

    }
}