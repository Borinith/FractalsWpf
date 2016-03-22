using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FractalsWpf
{
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly int[] ColourMap = ColourMaps.GetColourMap("jet");
        //private readonly IFractals _mandelbrotSet = new MandelbrotSet();
        private readonly IFractals _mandelbrotSetGpu = new MandelbrotSetGpu();
        //private readonly IFractals _juliaSet = new JuliaSet();
        //private readonly IFractals _juliaSetGpu = new JuliaSetGpu();
        private int _fractalImageWidth;
        private int _fractalImageHeight;
        private WriteableBitmap _writeableBitmap;
        private int _maxIterations;
        private int _zoomLevel;
        private Complex _bottomLeft;
        private Complex _topRight;
        private IFractals _fractals;
        private bool _initDone;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ContentRendered += (_, __) =>
            {
                _fractalImageWidth = (int)Math.Round(FractalImageWrapper.ActualWidth);
                _fractalImageHeight = (int)Math.Round(FractalImageWrapper.ActualHeight);

                _writeableBitmap = new WriteableBitmap(
                    _fractalImageWidth,
                    _fractalImageHeight,
                    96,
                    96,
                    PixelFormats.Bgr32,
                    null);

                FractalImage.Source = _writeableBitmap;

                MaxIterations = 120;
                ZoomLevel = 25;

                //_bottomLeft = new Complex(-2d, -2d);
                //_topRight = new Complex(2d, 2d);

                //_bottomLeft = new Complex(-2.25d, -1.5d);
                //_topRight = new Complex(0.75d, 1.5d);

                //_bottomLeft = new Complex(-1.5d, -0.5d);
                //_topRight = new Complex(-0.5d, 0.5d);

                //_bottomLeft = new Complex(-0.0d, -0.9d);
                //_topRight = new Complex(0.6d, -0.3d);

                _bottomLeft = new Complex(-0.22d, -0.70d);
                _topRight = new Complex(-0.21d, -0.69d);

                _fractals = _mandelbrotSetGpu;

                Render();

                _initDone = true;
            };

            ZoomLevelSlider.ValueChanged += (_, args) =>
            {
                if (_initDone)
                {
                    var diff = args.NewValue - args.OldValue;

                    foreach (var idx in Enumerable.Range(0, (int)Math.Abs(diff)))
                    {
                        if (diff > 0)
                        {
                            var w = _topRight.Real - _bottomLeft.Real;
                            var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                            var dw = w / 4;
                            var dh = h / 4;
                            _bottomLeft = new Complex(_bottomLeft.Real + dw, _bottomLeft.Imaginary + dh);
                            _topRight = new Complex(_topRight.Real - dw, _topRight.Imaginary - dh);
                        }
                        else
                        {
                            var w = _topRight.Real - _bottomLeft.Real;
                            var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                            var dw = w / 2;
                            var dh = h / 2;
                            _bottomLeft = new Complex(_bottomLeft.Real - dw, _bottomLeft.Imaginary - dh);
                            _topRight = new Complex(_topRight.Real + dw, _topRight.Imaginary + dh);
                        }
                    }

                    Render();
                }
            };

            MaxIterationsSlider.ValueChanged += (_, __) =>
            {
                if (_initDone)
                {
                    Render();
                }
            };

            ZoomInBtn.Click += (_, __) =>
            {
                var w = _topRight.Real - _bottomLeft.Real;
                var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                var dw = w/4;
                var dh = h/4;
                _bottomLeft = new Complex(_bottomLeft.Real + dw, _bottomLeft.Imaginary + dh);
                _topRight = new Complex(_topRight.Real - dw, _topRight.Imaginary - dh);
                Render();
            };

            ZoomOutBtn.Click += (_, __) =>
            {
                var w = _topRight.Real - _bottomLeft.Real;
                var h = _topRight.Imaginary - _bottomLeft.Imaginary;
                var dw = w/2;
                var dh = h/2;
                _bottomLeft = new Complex(_bottomLeft.Real - dw, _bottomLeft.Imaginary - dh);
                _topRight = new Complex(_topRight.Real + dw, _topRight.Imaginary + dh);
                Render();
            };

            RenderBtn.Click += (_, __) => { Render(); };

            Closed += (_, __) =>
            {
                _fractals.Dispose();
            };
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
            var tuple = TimeIt(() => _fractals.CreatePixelArray(
                new Complex(-0.35, 0.65),
                _bottomLeft,
                _topRight,
                MaxIterations,
                _fractalImageWidth,
                _fractalImageHeight));

            var values = tuple.Item1;
            var elapsed = tuple.Item2;
            StatusBarText.Text = $"{_fractals.GetType().Name}: {elapsed}";

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
