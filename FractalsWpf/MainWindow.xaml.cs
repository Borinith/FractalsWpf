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
using MaterialDesignThemes.Wpf;

namespace FractalsWpf
{
    public enum FractalType
    {
        MandelbrotSet,
        JuliaSet,
        BarnsleyFern
    }

    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly int[] JetColourMap = ColourMaps.GetColourMap("jet");
        private static readonly int[] GistSternColourMap = ColourMaps.GetColourMap("gist_stern");
        private static readonly int[] ForestGreenColourMap = Enumerable.Repeat(Colors.White.ToInt(), 255).Concat(new[] {Colors.ForestGreen.ToInt()}).ToArray();
        private static readonly int[] MonochromeColourMap = Enumerable.Repeat(Colors.White.ToInt(), 255).Concat(new[] {Colors.Black.ToInt()}).ToArray();
        public static List<Tuple<string, int[]>> AvailableColourMaps => new List<Tuple<string, int[]>>
        {
            Tuple.Create("Jet", JetColourMap),
            Tuple.Create("GistStern", GistSternColourMap),
            Tuple.Create("ForestGreen", ForestGreenColourMap),
            Tuple.Create("Monochrome", MonochromeColourMap)
        };
        private readonly IFractal _mandelbrotSetGpuFloat = new MandelbrotSetGpuFloat();
        private readonly IFractal _mandelbrotSetGpuDouble = new MandelbrotSetGpuDouble();
        private readonly IFractal _juliaSetGpuFloat = new JuliaSetGpuFloat();
        private readonly IFractal _juliaSetGpuDouble = new JuliaSetGpuDouble();
        private readonly IFractal _barnsleyFern = new BarnsleyFern();
        private IFractal _selectedFractal;
        private FractalType _fractalType;
        private Point _juliaConstant = new Point(-0.35, 0.65);
        private bool _isGpuDataTypeDouble;
        private int _fractalImageWidth;
        private int _fractalImageHeight;
        private WriteableBitmap _writeableBitmap;
        private int _maxIterations;
        private int _previousZoomLevel;
        private int _zoomLevel;
        private Point _bottomLeft;
        private Point _topRight;
        private bool _initDone;
        private int[] _selectedColourMap;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ContentRendered += (_, __) =>
            {
                //BottomLeft = new Point(-2d, -2d);
                //TopRight = new Point(2d, 2d);

                BottomLeft = new Point(-2.25d, -1.5d);
                TopRight = new Point(0.75d, 1.5d);

                //BottomLeft = new Point(-1.5d, -0.5d);
                //TopRight = new Point(-0.5d, 0.5d);

                //BottomLeft = new Point(-0.0d, -0.9d);
                //TopRight = new Point(0.6d, -0.3d);

                //BottomLeft = new Point(-0.22d, -0.70d);
                //TopRight = new Point(-0.21d, -0.69d);

                //BottomLeft = new Point(-3d, -1d);
                //TopRight = new Point(3d, 11d);

                AdjustAspectRatio();

                MaxIterations = 120;
                ZoomLevel = 1;
                _previousZoomLevel = 1;
                IsMandelbrotSet = true;
                IsGpuDataTypeDouble = true;
                SelectedColourMap = AvailableColourMaps[0].Item2;

                _initDone = true;

                Render();
            };

            ZoomLevelSlider.ValueChanged += (_, args) =>
            {
                if (!_initDone) return;

                if (ZoomLevel == _previousZoomLevel) return;

                var diff = ZoomLevel - _previousZoomLevel;

                foreach (var idx in Enumerable.Range(0, Math.Abs(diff)))
                {
                    var w = TopRight.X - BottomLeft.X;
                    var h = TopRight.Y - BottomLeft.Y;

                    if (diff > 0)
                    {
                        var dw = w / 4;
                        var dh = h / 4;
                        BottomLeft = new Point(BottomLeft.X + dw, BottomLeft.Y + dh);
                        TopRight = new Point(TopRight.X - dw, TopRight.Y - dh);
                    }
                    else
                    {
                        var dw = w / 2;
                        var dh = h / 2;
                        BottomLeft = new Point(BottomLeft.X - dw, BottomLeft.Y - dh);
                        TopRight = new Point(TopRight.X + dw, TopRight.Y + dh);
                    }
                }

                Render();

                _previousZoomLevel = ZoomLevel;
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

            MouseDown += (_, args) =>
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    var mousePt = Mouse.GetPosition(FractalImage);
                    var regionWidth = TopRight.X - BottomLeft.X;
                    var regionHeight = TopRight.Y - BottomLeft.Y;
                    var regionCentreX = BottomLeft.X + regionWidth/2;
                    var regionCentreY = BottomLeft.Y + regionHeight/2;
                    var regionMouseX = mousePt.X*regionWidth/_fractalImageWidth + BottomLeft.X;
                    var regionMouseY = mousePt.Y*regionHeight/_fractalImageHeight + BottomLeft.Y;
                    var dx = regionMouseX - regionCentreX;
                    var dy = regionMouseY - regionCentreY;
                    BottomLeft = new Point(BottomLeft.X + dx, BottomLeft.Y + dy);
                    TopRight = new Point(TopRight.X + dx, TopRight.Y + dy);
                    Render();
                    return;
                }

                lastMousePt = Mouse.GetPosition(FractalImage);
                panningInProgress = true;
            };

            MouseMove += (_, __) =>
            {
                if (!panningInProgress) return;
                var currentMousePt = Mouse.GetPosition(FractalImage);
                var mouseDx = currentMousePt.X - lastMousePt.X;
                var mouseDy = currentMousePt.Y - lastMousePt.Y;
                var regionWidth = TopRight.X - BottomLeft.X;
                var regionHeight = TopRight.Y - BottomLeft.Y;
                var regionDx = mouseDx / _fractalImageWidth * regionWidth;
                var regionDy = mouseDy / _fractalImageHeight * regionHeight;
                BottomLeft = new Point(BottomLeft.X - regionDx, BottomLeft.Y - regionDy);
                TopRight = new Point(TopRight.X - regionDx, TopRight.Y - regionDy);
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

            MouseRightButtonDown += (_, __) =>
            {
                if (IsMandelbrotSet)
                {
                    var mousePt = Mouse.GetPosition(FractalImage);
                    var regionWidth = TopRight.X - BottomLeft.X;
                    var regionHeight = TopRight.Y - BottomLeft.Y;
                    var regionMouseX = mousePt.X * regionWidth / _fractalImageWidth + BottomLeft.X;
                    var regionMouseY = mousePt.Y * regionHeight / _fractalImageHeight + BottomLeft.Y;
                    _juliaConstant = new Point(regionMouseX, regionMouseY);
                    IsJuliaSet = true;
                }
            };

            Closed += (_, __) =>
            {
                _selectedFractal.Dispose();
            };

            SetRegionBtn.Click += async (_, __) =>
            {
                var setRegionDialog = new SetRegionDialog();
                var result = (bool)await DialogHost.Show(setRegionDialog, "RootDialog", SetRegionDialogClosing);
                if (!result) return;
                BottomLeft = new Point(setRegionDialog.BottomLeftX, setRegionDialog.BottomLeftY);
                TopRight = new Point(setRegionDialog.TopRightX, setRegionDialog.TopRightY);
                AdjustAspectRatio();
                Render();
            };
        }

        private static void SetRegionDialogClosing(object _, DialogClosingEventArgs __)
        {
        }

        private void AdjustAspectRatio()
        {
            if (_fractalImageWidth > _fractalImageHeight)
            {
                var regionWidth = TopRight.X - BottomLeft.X;
                var newRegionWidthDiff = _fractalImageWidth*regionWidth/_fractalImageHeight - regionWidth;
                var halfNewRegionWidthDiff = newRegionWidthDiff/2;
                BottomLeft = new Point(BottomLeft.X - halfNewRegionWidthDiff, BottomLeft.Y);
                TopRight = new Point(TopRight.X + halfNewRegionWidthDiff, TopRight.Y);
            }
            else if (_fractalImageHeight > _fractalImageWidth)
            {
                var regionHeight = TopRight.Y - BottomLeft.Y;
                var newRegionHeightDiff = _fractalImageHeight*regionHeight/_fractalImageWidth - regionHeight;
                var halfNewRegionHeightDiff = newRegionHeightDiff/2;
                BottomLeft = new Point(BottomLeft.X, BottomLeft.Y - halfNewRegionHeightDiff);
                TopRight = new Point(TopRight.X, TopRight.Y + halfNewRegionHeightDiff);
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

        public Point BottomLeft {
            get { return _bottomLeft; }
            set
            {
                _bottomLeft = value;
                SetStatusBarRightText();
                OnPropertyChanged();
            }
        }

        public Point TopRight {
            get { return _topRight; }
            set
            {
                _topRight = value;
                SetStatusBarRightText();
                OnPropertyChanged();
            }
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

        public bool IsMandelbrotSet
        {
            get { return _fractalType == FractalType.MandelbrotSet; }
            set
            {
                _fractalType = FractalType.MandelbrotSet;
                OnPropertyChanged("IsMandelbrotSet");
                OnPropertyChanged("IsJuliaSet");
                OnPropertyChanged("IsBarnsleyFern");
                UpdateSelectedFractal();
                Render();
            }
        }

        public bool IsJuliaSet
        {
            get { return _fractalType == FractalType.JuliaSet; }
            set
            {
                _fractalType = FractalType.JuliaSet;
                OnPropertyChanged("IsMandelbrotSet");
                OnPropertyChanged("IsJuliaSet");
                OnPropertyChanged("IsBarnsleyFern");
                UpdateSelectedFractal();
                Render();
            }
        }

        public bool IsBarnsleyFern
        {
            get { return _fractalType == FractalType.BarnsleyFern; }
            set
            {
                _fractalType = FractalType.BarnsleyFern;
                OnPropertyChanged("IsMandelbrotSet");
                OnPropertyChanged("IsJuliaSet");
                OnPropertyChanged("IsBarnsleyFern");
                UpdateSelectedFractal();
                BottomLeft = new Point(-3d, -1d);
                TopRight = new Point(3d, 11d);
                MaxIterations = 120000;
                Render();
            }
        }

        public bool IsGpuDataTypeFloat {
            get { return !_isGpuDataTypeDouble; }
            set
            {
                _isGpuDataTypeDouble = !value;
                OnPropertyChanged();
                UpdateSelectedFractal();
                Render();
            }
        }

        public bool IsGpuDataTypeDouble {
            get { return _isGpuDataTypeDouble; }
            set
            {
                _isGpuDataTypeDouble = value;
                OnPropertyChanged();
                UpdateSelectedFractal();
                Render();
            }
        }

        public int[] SelectedColourMap
        {
            get { return _selectedColourMap; }
            set
            {
                _selectedColourMap = value;
                OnPropertyChanged();
                Render();
            }
        }

        private void UpdateSelectedFractal()
        {
            var dict = new Dictionary<Tuple<FractalType, bool>, IFractal>
            {
                { Tuple.Create(FractalType.MandelbrotSet, true), _mandelbrotSetGpuDouble},
                { Tuple.Create(FractalType.MandelbrotSet, false), _mandelbrotSetGpuFloat},
                { Tuple.Create(FractalType.JuliaSet, true), _juliaSetGpuDouble},
                { Tuple.Create(FractalType.JuliaSet, false), _juliaSetGpuFloat},
                { Tuple.Create(FractalType.BarnsleyFern, true), _barnsleyFern},
                { Tuple.Create(FractalType.BarnsleyFern, false), _barnsleyFern}
            };

            _selectedFractal = dict[Tuple.Create(_fractalType, _isGpuDataTypeDouble)];
        }

        private void SetStatusBarLeftText(TimeSpan elapsedTime1, TimeSpan elapsedTime2, TimeSpan elapsedTime3)
        {
            StatusBarLeftText.Text = $"{_selectedFractal.GetType().Name}: {elapsedTime1.TotalMilliseconds}ms; {elapsedTime2.TotalMilliseconds}ms; {elapsedTime3.TotalMilliseconds}ms";
        }

        private void SetStatusBarRightText()
        {
            StatusBarRightText.Text = $"Region: ({BottomLeft.X}, {BottomLeft.Y}) ({TopRight.X}, {TopRight.Y})";
        }

        private void Render()
        {
            if (!_initDone) return;

            var tuple1 = TimeIt(() => _selectedFractal.CreatePixelArray(
                new Complex(_juliaConstant.X, _juliaConstant.Y),
                new Complex(BottomLeft.X, BottomLeft.Y), 
                new Complex(TopRight.X, TopRight.Y), 
                _fractalImageWidth,
                _fractalImageHeight,
                MaxIterations));
            var values = tuple1.Item1;
            var elapsedTime1 = tuple1.Item2;

            var tuple2 = TimeIt(() => ValuesToPixels(values, _selectedColourMap));
            var pixels = tuple2.Item1;
            var elapsedTime2 = tuple2.Item2;

            var elapsedTime3 = TimeIt(() =>
            {
                var sourceRect = new Int32Rect(0, 0, _fractalImageWidth, _fractalImageHeight);
                _writeableBitmap.WritePixels(sourceRect, pixels, _writeableBitmap.BackBufferStride, 0);
            });

            SetStatusBarLeftText(elapsedTime1, elapsedTime2, elapsedTime3);
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

        private static Tuple<T, TimeSpan> TimeIt<T>(Func<T> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();
            return Tuple.Create(result, stopwatch.Elapsed);
        }

        private static TimeSpan TimeIt(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
