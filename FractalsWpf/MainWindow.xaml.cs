using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics;

namespace FractalsWpf
{
    public partial class MainWindow
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private int _fractalImageWidth;
        private int _fractalImageHeight;
        private WriteableBitmap _writeableBitmap;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) =>
            {
                _fractalImageWidth = (int)Math.Round(Grid.ActualWidth);
                _fractalImageHeight = (int)Math.Round(Grid.ActualHeight);

                _writeableBitmap = new WriteableBitmap(
                    _fractalImageWidth,
                    _fractalImageHeight,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                var sourceRect = new Int32Rect(0, 0, _fractalImageWidth, _fractalImageHeight);
                const double x1 = -2.25d;
                const double x2 = 0.75d;
                const double y1 = -1.5d;
                const double y2 = 1.5d;
                var sourceBuffer = MakeSourceBuffer(
                    new Rect(x1, y1, x2 - x1, y2 - y1), 
                    _fractalImageWidth,
                    _fractalImageHeight);
                _writeableBitmap.WritePixels(sourceRect, sourceBuffer, _writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = _writeableBitmap;
            };
        }

        private static int[] MakeSourceBuffer(Rect rect, int pixelWidth, int pixelHeight)
        {
            const int maxIterations = 40;

            var realValues = Generate.LinearSpaced(pixelWidth, rect.Left, rect.Right);
            var imaginaryValues = Generate.LinearSpaced(pixelHeight, rect.Bottom, rect.Top);

            var pixels =
                from imaginary in imaginaryValues
                from real in realValues
                let c = new Complex(real, imaginary)
                let m = Mandel(c, maxIterations)
                select MapColour(m, maxIterations);

            return pixels.ToArray();
        }

        private static int Mandel(Complex c, int maxIterations)
        {
            var z = new Complex(0d, 0d);

            foreach (var iterations in Enumerable.Range(0, maxIterations))
            {
                z = z*z + c;
                if (z.Magnitude >= 4d) return iterations;
            }

            return maxIterations;
        }

        private static int MapColour(int iterations, int maxIterations)
        {
            return iterations < maxIterations ? 0x00FFFFFF : 0x00000000;
        }
    }
}
