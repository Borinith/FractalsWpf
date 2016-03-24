using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FractalsWpf
{
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly int[] ColourMap = ColourMaps.GetColourMap("jet");
        private readonly IFractal _mandelbrotSetGpuFloat = new MandelbrotSetGpuFloat();
        private readonly IFractal _mandelbrotSetGpuDouble = new MandelbrotSetGpuDouble();
        private readonly IFractal _juliaSetGpuFloat = new JuliaSetGpuFloat();
        private readonly IFractal _juliaSetGpuDouble = new JuliaSetGpuDouble();
        private readonly IFractal _barnsleyFern = new BarnsleyFern();
        private IFractal _fractal;
        private int _fractalImageWidth;
        private int _fractalImageHeight;
        private WriteableBitmap _writeableBitmap;
        private int _maxIterations;
        private int _zoomLevel;
        private Complex _bottomLeft;
        private Complex _topRight;
        private bool _initDone;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ContentRendered += (_, __) =>
            {
                //_bottomLeft = new Complex(-2d, -2d);
                //_topRight = new Complex(2d, 2d);

                _bottomLeft = new Complex(-2.25d, -1.5d);
                _topRight = new Complex(0.75d, 1.5d);

                //_bottomLeft = new Complex(-1.5d, -0.5d);
                //_topRight = new Complex(-0.5d, 0.5d);

                //_bottomLeft = new Complex(-0.0d, -0.9d);
                //_topRight = new Complex(0.6d, -0.3d);

                //_bottomLeft = new Complex(-0.22d, -0.70d);
                //_topRight = new Complex(-0.21d, -0.69d);

                //_bottomLeft = new Complex(-3d, -1d);
                //_topRight = new Complex(3d, 11d);

                AdjustAspectRatio();

                MaxIterations = 120;
                ZoomLevel = 1;

                _fractal = _mandelbrotSetGpuDouble;
                _initDone = true;

                Render();
            };

            ZoomLevelSlider.ValueChanged += (_, args) =>
            {
                var diff = args.NewValue - args.OldValue;

                foreach (var idx in Enumerable.Range(0, (int)Math.Abs(diff)))
                {
                    if (diff > 0)
                    {
                        var w = _topRight.Real - _bottomLeft.Real;
                        var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                        var dw = w / 8;
                        var dh = h / 8;
                        _bottomLeft = new Complex(_bottomLeft.Real + dw, _bottomLeft.Imaginary + dh);
                        _topRight = new Complex(_topRight.Real - dw, _topRight.Imaginary - dh);
                    }
                    else
                    {
                        var w = _topRight.Real - _bottomLeft.Real;
                        var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                        var dw = w / 4;
                        var dh = h / 4;
                        _bottomLeft = new Complex(_bottomLeft.Real - dw, _bottomLeft.Imaginary - dh);
                        _topRight = new Complex(_topRight.Real + dw, _topRight.Imaginary + dh);
                    }
                }

                Render();
            };

            MaxIterationsSlider.ValueChanged += (_, __) =>
            {
                Render();
            };

            SizeChanged += (_, __) =>
            {
                _fractalImageWidth = (int)Math.Floor(FractalImageWrapper.ActualWidth);
                _fractalImageHeight = (int)Math.Floor(FractalImageWrapper.ActualHeight);
                //AdjustAspectRatio();
                _writeableBitmap = new WriteableBitmap(
                    _fractalImageWidth,
                    _fractalImageHeight,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                FractalImage.Source = _writeableBitmap;
                Render();
            };

            var lastMousePt = new Point();
            var panningInProgress = false;

            MouseDown += (_, __) =>
            {
                lastMousePt = Mouse.GetPosition(FractalImage);
                panningInProgress = true;
            };

            MouseMove += (_, __) =>
            {
                if (!panningInProgress) return;
                var currentMousePt = Mouse.GetPosition(FractalImage);
                var mouseDx = currentMousePt.X - lastMousePt.X;
                var mouseDy = currentMousePt.Y - lastMousePt.Y;
                var regionWidth = _topRight.Real - _bottomLeft.Real;
                var regionHeight = _topRight.Imaginary - _bottomLeft.Imaginary;
                var regionDx = mouseDx / _fractalImageWidth * regionWidth;
                var regionDy = mouseDy / _fractalImageHeight * regionHeight;
                _bottomLeft = new Complex(_bottomLeft.Real - regionDx, _bottomLeft.Imaginary - regionDy);
                _topRight = new Complex(_topRight.Real - regionDx, _topRight.Imaginary - regionDy);
                Render();
                lastMousePt = currentMousePt;
            };

            MouseUp += (_, __) =>
            {
                panningInProgress = false;
            };

            MouseLeave += (_, __) =>
            {
                panningInProgress = false;
            };

            Closed += (_, __) =>
            {
                _fractal.Dispose();
            };
        }

        private void AdjustAspectRatio()
        {
            if (_fractalImageWidth > _fractalImageHeight)
            {
                var regionWidth = _topRight.Real - _bottomLeft.Real;
                var newRegionWidthDiff = _fractalImageWidth*regionWidth/_fractalImageHeight - regionWidth;
                var halfNewRegionWidthDiff = newRegionWidthDiff/2;
                _bottomLeft = new Complex(_bottomLeft.Real - halfNewRegionWidthDiff, _bottomLeft.Imaginary);
                _topRight = new Complex(_topRight.Real + halfNewRegionWidthDiff, _topRight.Imaginary);
            }
            else if (_fractalImageHeight > _fractalImageWidth)
            {
                var regionHeight = _topRight.Imaginary - _bottomLeft.Imaginary;
                var newRegionHeightDiff = _fractalImageHeight*regionHeight/_fractalImageWidth - regionHeight;
                var halfNewRegionHeightDiff = newRegionHeightDiff/2;
                _bottomLeft = new Complex(_bottomLeft.Real, _bottomLeft.Imaginary - halfNewRegionHeightDiff);
                _topRight = new Complex(_topRight.Real, _topRight.Imaginary + halfNewRegionHeightDiff);
            }

            _writeableBitmap = new WriteableBitmap(
                _fractalImageWidth,
                _fractalImageHeight,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            FractalImage.Source = _writeableBitmap;
        }

        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                _maxIterations = value;
                OnPropertyChanged();
            }
        }

        public int ZoomLevel
        {
            get { return _zoomLevel; }
            set
            {
                _zoomLevel = value;
                OnPropertyChanged();
            }
        }

        private void Render()
        {
            if (!_initDone) return;

            var tuple = TimeIt(() => _fractal.CreatePixelArray(
                new Complex(-0.35, 0.65),
                _bottomLeft,
                _topRight,
                _fractalImageWidth,
                _fractalImageHeight,
                MaxIterations));

            var values = tuple.Item1;
            var elapsed = tuple.Item2;
            StatusBarText.Text = $"{_fractal.GetType().Name}: {elapsed.TotalMilliseconds}ms";

            //var pixels = BarnsleyFern.CreatePixelArray(
            //    fractalImageWidth,
            //    fractalImageHeight,
            //    Colors.ForestGreen.ToInt(),
            //    10000000);

            var pixels = ValuesToPixels(values, ColourMap);

            var sourceRect = new Int32Rect(0, 0, _fractalImageWidth, _fractalImageHeight);
            _writeableBitmap.WritePixels(sourceRect, pixels, _writeableBitmap.BackBufferStride, 0);
        }

        private static int[] ValuesToPixels(ushort[] values, IReadOnlyList<int> colourMap)
        {
            var lastIndex = colourMap.Count - 1;
            var vmin = (double)values.Min();
            var vmax = (double)values.Max();
            var divisor = vmax - vmin;
            var normalisedValues = values
                .Select(p => (p - vmin)/divisor)
                .Select(p => double.IsNaN(p) ? 0d : p)
                .ToArray();
            return normalisedValues.Select(p => colourMap[(int)Math.Floor(p * lastIndex)]).ToArray();
        }

        private static Tuple<T, TimeSpan> TimeIt<T>(Func<T> f)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = f();
            stopwatch.Stop();
            return Tuple.Create(result, stopwatch.Elapsed);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
