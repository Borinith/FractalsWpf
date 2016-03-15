using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics;

namespace FractalsWpf
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) =>
            {
                var fractalImageWidth = (int)Math.Round(Grid.ActualWidth);
                var fractalImageHeight = (int)Math.Round(Grid.ActualHeight);

                var writeableBitmap = new WriteableBitmap(
                    fractalImageWidth,
                    fractalImageHeight,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                //var bottomLeft = new Complex(-2.25d, -1.5d);
                //var topRight = new Complex(0.75d, 1.5d);

                //var bottomLeft = new Complex(-1.5d, -0.5d);
                //var topRight = new Complex(-0.5d, 0.5d);

                //var bottomLeft = new Complex(-0.0d, -0.9d);
                //var topRight = new Complex(0.6d, -0.3d);

                var bottomLeft = new Complex(-0.22d, -0.70d);
                var topRight = new Complex(-0.21d, -0.69d);

                const int maxIterations = 255;
                var colourTable = CreateColourTable(maxIterations);

                IFractals fractals = new FractalsRegular();
                var pixels = fractals.CreatePixelArray(
                    bottomLeft,
                    topRight, 
                    colourTable,
                    fractalImageWidth,
                    fractalImageHeight);

                var sourceRect = new Int32Rect(0, 0, fractalImageWidth, fractalImageHeight);
                writeableBitmap.WritePixels(sourceRect, pixels, writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = writeableBitmap;
            };
        }

        private static int[] CreateColourTable(int maxIterations)
        {
            var startColour = Colors.Orange;
            var stopColour = Colors.Red;
            var start = startColour.ToInt();
            var stop = stopColour.ToInt();
            return Generate.LinearSpacedMap(maxIterations, start, stop, Convert.ToInt32);
        }
    }
}
