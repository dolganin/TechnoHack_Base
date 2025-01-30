using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace SignalLabelingApp.Views
{

    public partial class SpectrogramView : UserControl
    {
        private List<float> _spectrumData = new();
        private object _lock = new();
        private float _maxAmplitude = 10000.0f;
        private float _maxFrequency = 50.0f;

        public SpectrogramView()
        {
            InitializeComponent();

            this.ClipToBounds = true;
        }

        public void Update(List<float> signal, float samplingRate)
        {
            if (signal == null || signal.Count == 0)
                return;

            int n = signal.Count;

            // Вычисление среднего значения
            float mean = signal.Average();

            // Центровка сигнала
            var centeredSignal = signal.Select(v => v - mean).ToArray();

            // Применение окна Ханна
            double[] window = Enumerable.Range(0, n)
                .Select(i => 0.5 * (1 - Math.Cos(2 * Math.PI * i / (n - 1))))
                .ToArray();

            Complex[] complexSignal = signal.Select(v => new Complex(v, 0)).ToArray();

            //Complex[] complexSignal = centeredSignal
            //    .Select((v, i) => new Complex(v * window[i], 0))
            //    .ToArray();

            // Применение БПФ
            Fourier.Forward(complexSignal, FourierOptions.Matlab);

            // Установка корректного maxFrequency
            lock (_lock)
            {
                _spectrumData = complexSignal.Skip(1).Take(n / 2 - 1)
                    .Select(c => (float)c.Magnitude)
                    .ToList();

                _maxAmplitude = _spectrumData.Max();
                _maxFrequency = samplingRate / 2.0f; // Правильный расчет максимальной частоты
            }

            InvalidateVisual();
        }



        public override void Render(DrawingContext context)
        {
            base.Render(context);
            lock (_lock)
            {
                var size = this.Bounds.Size;
                if (_spectrumData.Count == 0 || size.Width == 0 || size.Height == 0)
                    return;

                var pen = new Pen(Brushes.Black, 2);
                var axisPen = new Pen(Brushes.Black, 1);
                var textBrush = Brushes.Black;
                var textFont = new Typeface("Arial");
                var textSize = 12;

                float width = (float)size.Width;
                float height = (float)size.Height;
                float xOffset = 40; // Смещение для осей
                float yOffset = 20;

                // Рисуем спектр
                for (int i = 1; i < _spectrumData.Count; i++)
                {
                    float x1 = xOffset + (i - 1) / _maxFrequency * (width - xOffset);
                    float y1 = height - yOffset - (_spectrumData[i - 1] / _maxAmplitude * (height - yOffset));
                    float x2 = xOffset + i / _maxFrequency * (width - xOffset);
                    float y2 = height - yOffset - (_spectrumData[i] / _maxAmplitude * (height - yOffset));

                    context.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
                }

                // Рисуем оси
                context.DrawLine(axisPen, new Point(xOffset, height - yOffset), new Point(width, height - yOffset)); // Горизонтальная ось
                context.DrawLine(axisPen, new Point(xOffset, 0), new Point(xOffset, height - yOffset)); // Вертикальная ось

                // Подписываем оси
                for (int i = 0; i <= 5; i++)
                {
                    float x = xOffset + i * (width - xOffset) / 5;
                    float freq = i * (_maxFrequency / 5); // Теперь частота рассчитывается корректно
                    var formattedText = new FormattedText(
                        $"{freq:F1}",
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        textFont,
                        textSize,
                        textBrush
                    );
                    context.DrawText(formattedText, new Point(x - 10, height - yOffset + 5));
                }

                for (int i = 0; i <= 5; i++)
                {
                    float y = height - yOffset - i * (height - yOffset) / 5;
                    float amp = (i * _maxAmplitude / 5);

                    var formattedText = new FormattedText(
                        $"{amp:F1}",
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        textFont,
                        textSize,
                        textBrush
                    );
                    context.DrawText(formattedText, new Point(5, y - 5));
                }
            }
        }
    }
}