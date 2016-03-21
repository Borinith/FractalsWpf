using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FractalsWpf
{
    public partial class MainWindow
    {
        private static readonly int[] ColourMap = ColourMaps.GetColourMap("jet");
        private IFractals _mandelbrotSetGpu = new MandelbrotSetGpu();

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

                //var bottomLeft = new Complex(-2d, -2d);
                //var topRight = new Complex(2d, 2d);

                //var bottomLeft = new Complex(-2.25d, -1.5d);
                //var topRight = new Complex(0.75d, 1.5d);

                //var bottomLeft = new Complex(-1.5d, -0.5d);
                //var topRight = new Complex(-0.5d, 0.5d);

                //var bottomLeft = new Complex(-0.0d, -0.9d);
                //var topRight = new Complex(0.6d, -0.3d);

                var bottomLeft = new Complex(-0.22d, -0.70d);
                var topRight = new Complex(-0.21d, -0.69d);

                const int maxIterations = 120;

                IFractals mandelbrotSetNonGpu = new MandelbrotSetNonGpu();
                IFractals juliaSetNonGpu = new JuliaSetNonGpu();

                //IFractals fractals = mandelbrotSetNonGpu;
                IFractals fractals = _mandelbrotSetGpu;
                //IFractals fractals = juliaSetNonGpu;

                var tuple = TimeIt(() => fractals.CreatePixelArray(
                    new Complex(-0.35, 0.65),
                    bottomLeft,
                    topRight,
                    maxIterations,
                    fractalImageWidth,
                    fractalImageHeight));

                var values = tuple.Item1;
                var elapsed = tuple.Item2;
                StatusBarText.Text = $"{fractals.GetType().Name}: {elapsed}";

                //var pixels = BarnsleyFern.CreatePixelArray(
                //    fractalImageWidth,
                //    fractalImageHeight,
                //    Colors.ForestGreen.ToInt(),
                //    10000000);

                var pixels = ValuesToPixels(values);

                var sourceRect = new Int32Rect(0, 0, fractalImageWidth, fractalImageHeight);
                writeableBitmap.WritePixels(sourceRect, pixels, writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = writeableBitmap;
            };
        }

        private static int[] ValuesToPixels(int[] values)
        {
            var vmin = (double)values.Min();
            var vmax = (double)values.Max();
            var divisor = vmax - vmin;
            var normalisedValues = values.Select(p => (p - vmin) / divisor).ToArray();
            return normalisedValues.Select(p => ColourMap[(int)Math.Floor(p * 255)]).ToArray();
        }

        private static Tuple<T, TimeSpan> TimeIt<T>(Func<T> f)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = f();
            stopwatch.Stop();
            return Tuple.Create(result, stopwatch.Elapsed);
        }
    }
}
