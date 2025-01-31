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

        public OxControl()
        {
            InitializeComponent();
            // maxValue = MaxValue;
            // middleValue = MiddleValue;
            // minValue = MinValue;
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
            double arrowOxHeight = canvasHeight * 0.4;

            var geometryGroupOx = new GeometryGroup
            {
                Children = new GeometryCollection
                {
                    // Вертикальная стрелка
                    new LineGeometry(new Point(0, canvasHeight / 2), new Point(canvasWidth, canvasHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.975, canvasHeight / 2 -  arrowOxHeight / 2 ), new Point(arrowOxWidth, canvasHeight / 2 )),
                    new LineGeometry(new Point(arrowOxWidth * 0.975, canvasHeight / 2 +  arrowOxHeight / 2 ), new Point(arrowOxWidth, canvasHeight / 2)),

                    // Узкие деления
                    new LineGeometry(new Point(arrowOxWidth * 0, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.1, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.1, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.2, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.2, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.3, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.3, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.4, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.4, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.5, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.5, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.6, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.6, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.7, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.7, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.8, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.8, canvasHeight / 2 + arrowOxHeight / 2)),
                    new LineGeometry(new Point(arrowOxWidth * 0.9, canvasHeight / 2 - arrowOxHeight / 2), new Point(arrowOxWidth * 0.9, canvasHeight / 2 + arrowOxHeight / 2)),
                    
                    
                }
            };

            ArrowOxPath.Data = geometryGroupOx;
        }
    

    }
}