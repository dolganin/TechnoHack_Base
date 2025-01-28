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
    public partial class OneChannelControl: UserControl 
    {
        public OneChannelControl()
        {

        }

        private void RedrawOneChannel(TraceData trace, double Xscale,  double startX, double endX, ImmutableSolidColorBrush brush)
        {
            OneChannelCanvas.Children.Clear();


            if (trace == null || trace.data == null || trace.data.Count == 0)
                return;

            var polyline = new Polyline
            {
                Stroke = brush,
                StrokeThickness = 1
            };
            

            int startIndex = Math.Max(0, (int)(startX / Xscale));
            int endIndex = Math.Min(trace.data.Count, (int)(endX / Xscale));

            float maxValue = GetMaxInRange(trace, startIndex, endIndex);
            float minValue = GetMinInRange(trace, startIndex, endIndex);

            if (maxValue == 0)
                return;

            float centerY = (float)(OneChannelCanvas.Bounds.Height) / 2;

            for (int i = startIndex; i < endIndex; i++)
            {
                float x = i * (float)(Xscale);
                float y = centerY - (2 * (float)(trace.data[i] - minValue) / (maxValue - minValue) - 1) * ((float)(OneChannelCanvas.Bounds.Height) / 2 - 2);

                polyline.Points.Add(new Avalonia.Point(x, y));
                
            }

            OneChannelCanvas.Children.Add(polyline);
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