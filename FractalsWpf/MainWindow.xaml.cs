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
                //var bottomLeft = new Point(-2.25d, -1.5d);
                //var topRight = new Point(0.75d, 1.5d);
                //var bottomLeft = new Point(-1.5d, -0.5d);
                //var topRight = new Point(-0.5d, 0.5d);
                //var bottomLeft = new Point(-0.0d, -0.9d);
                //var topRight = new Point(0.6d, -0.3d);
                var bottomLeft = new Point(-0.22d, -0.70d);
                var topRight = new Point(-0.21d, -0.69d);
                var sourceBuffer = MakeSourceBuffer(
                    new Rect(bottomLeft, topRight), 
                    _fractalImageWidth,
                    _fractalImageHeight);
                _writeableBitmap.WritePixels(sourceRect, sourceBuffer, _writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = _writeableBitmap;
            };
        }

        private static int[] MakeSourceBuffer(Rect rect, int pixelWidth, int pixelHeight)
        {
            const int maxIterations = 100;

            var realValues = Generate.LinearSpaced(pixelWidth, rect.Left, rect.Right);
            var imaginaryValues = Generate.LinearSpaced(pixelHeight, rect.Top, rect.Bottom);

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
