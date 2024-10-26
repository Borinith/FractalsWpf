using FractalsWpf.CollatzConjecture;
using FractalsWpf.Enums;
using FractalsWpf.Interfaces;
using FractalsWpf.JuliaSet;
using FractalsWpf.MandelbrotSet;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// ReSharper disable ExplicitCallerInfoArgument

namespace FractalsWpf
{
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private const string IS_MANDELBROT_SET_TEXT = "IsMandelbrotSet";
        private const string IS_JULIA_SET_TEXT = "IsJuliaSet";
        private const string IS_BARNSLEY_FERN_TEXT = "IsBarnsleyFern";
        private const string IS_COLLATZ_CONJECTURE_TEXT = "IsCollatzConjecture";

        private static readonly int[] JetColourMap = ColourMaps.GetColourMap(ColourMapEnum.Jet);
        private static readonly int[] GistSternColourMap = ColourMaps.GetColourMap(ColourMapEnum.GistStern);
        private static readonly int[] OceanColourMap = ColourMaps.GetColourMap(ColourMapEnum.Ocean);
        private static readonly int[] RainbowColourMap = ColourMaps.GetColourMap(ColourMapEnum.Rainbow);
        private static readonly int[] GnuPlotColourMap = ColourMaps.GetColourMap(ColourMapEnum.GnuPlot);
        private static readonly int[] AfmHotColourMap = ColourMaps.GetColourMap(ColourMapEnum.AfmHot);
        private static readonly int[] GistHeatColourMap = ColourMaps.GetColourMap(ColourMapEnum.GistHeat);

        private static readonly int[] ForestGreenWhiteColourMap = Enumerable.Repeat(Colors.White.ToInt(), 255).Concat(new[] { Colors.ForestGreen.ToInt() }).ToArray();
        private static readonly int[] ForestGreenBlackColourMap = Enumerable.Repeat(Colors.Black.ToInt(), 255).Concat(new[] { Colors.ForestGreen.ToInt() }).ToArray();
        private static readonly int[] MonochromeColourMap = Enumerable.Repeat(Colors.White.ToInt(), 255).Concat(new[] { Colors.Black.ToInt() }).ToArray();

        private readonly IFractal _barnsleyFern = new BarnsleyFern.BarnsleyFern();

        private readonly IFractal _collatzConjectureGpuDouble = new CollatzConjectureDouble();
        private readonly IFractal _collatzConjectureGpuFloat = new CollatzConjectureFloat();

        private readonly FrozenDictionary<(FractalTypeEnum FractalType, bool IsGpuDataTypeDouble), IFractal> _dict;

        private readonly Point _juliaConstant = new(-0.35, 0.65);
        private readonly IFractal _juliaSetGpuDouble = new JuliaSetGpuDouble();
        private readonly IFractal _juliaSetGpuFloat = new JuliaSetGpuFloat();

        private readonly IFractal _mandelbrotSetGpuDouble = new MandelbrotSetGpuDouble();
        private readonly IFractal _mandelbrotSetGpuFloat = new MandelbrotSetGpuFloat();

        private Point _bottomLeft;
        private int _fractalImageHeight;
        private int _fractalImageWidth;
        private FractalTypeEnum _fractalType;
        private bool _initDone;
        private bool _isGpuDataTypeDouble;
        private int _maxIterations;
        private int _previousZoomLevel;
        private int[] _selectedColourMap;
        private IFractal _selectedFractal;
        private Point _topRight;
        private WriteableBitmap _writeableBitmap;
        private int _zoomLevel;

        public MainWindow()
        {
            _dict = new Dictionary<(FractalTypeEnum FractalType, bool IsGpuDataTypeDouble), IFractal>
            {
                {
                    (FractalTypeEnum.MandelbrotSet, true), _mandelbrotSetGpuDouble
                },
                {
                    (FractalTypeEnum.MandelbrotSet, false), _mandelbrotSetGpuFloat
                },
                {
                    (FractalTypeEnum.JuliaSet, true), _juliaSetGpuDouble
                },
                {
                    (FractalTypeEnum.JuliaSet, false), _juliaSetGpuFloat
                },
                {
                    (FractalTypeEnum.BarnsleyFern, true), _barnsleyFern
                },
                {
                    (FractalTypeEnum.BarnsleyFern, false), _barnsleyFern
                },
                {
                    (FractalTypeEnum.CollatzConjecture, true), _collatzConjectureGpuDouble
                },
                {
                    (FractalTypeEnum.CollatzConjecture, false), _collatzConjectureGpuFloat
                }
            }.ToFrozenDictionary();

            InitializeComponent();
            DataContext = this;

            Closed += (_, __) => { (_selectedFractal as IFractalDisposable)?.Dispose(); };

            ContentRendered += (_, __) =>
            {
                _previousZoomLevel = 1;

                //BottomLeft = new Point(-2d, -2d);
                //TopRight = new Point(2d, 2d);

                //BottomLeft = new Point(-1.5d, -0.5d);
                //TopRight = new Point(-0.5d, 0.5d);

                //BottomLeft = new Point(-0.0d, -0.9d);
                //TopRight = new Point(0.6d, -0.3d);

                //BottomLeft = new Point(-0.22d, -0.70d);
                //TopRight = new Point(-0.21d, -0.69d);

                //AdjustAspectRatio();

                IsMandelbrotSet = true;
                IsGpuDataTypeDouble = true;

                _initDone = true;

                Render();
            };

            MaxIterationsSlider.ValueChanged += (_, __) => { Render(); };

            var lastMousePt = new Point();
            var panningInProgress = false;

            MouseDown += (_, __) =>
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    var mousePt = Mouse.GetPosition(FractalImage);
                    var regionWidth = TopRight.X - BottomLeft.X;
                    var regionHeight = TopRight.Y - BottomLeft.Y;
                    var regionCentreX = BottomLeft.X + regionWidth / 2;
                    var regionCentreY = BottomLeft.Y + regionHeight / 2;
                    var regionMouseX = mousePt.X * regionWidth / _fractalImageWidth + BottomLeft.X;
                    var regionMouseY = mousePt.Y * regionHeight / _fractalImageHeight + BottomLeft.Y;
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

            MouseLeave += (_, __) => { panningInProgress = false; };

            MouseMove += (_, __) =>
            {
                if (!panningInProgress)
                {
                    return;
                }

                var currentMousePt = Mouse.GetPosition(FractalImage);
                var mouseDx = currentMousePt.X - lastMousePt.X;
                var mouseDyTemp = currentMousePt.Y - lastMousePt.Y;
                var mouseDy = IsBarnsleyFern ? -mouseDyTemp : mouseDyTemp;
                var regionWidth = TopRight.X - BottomLeft.X;
                var regionHeight = TopRight.Y - BottomLeft.Y;
                var regionDx = mouseDx / _fractalImageWidth * regionWidth;
                var regionDy = mouseDy / _fractalImageHeight * regionHeight;
                BottomLeft = new Point(BottomLeft.X - regionDx, BottomLeft.Y - regionDy);
                TopRight = new Point(TopRight.X - regionDx, TopRight.Y - regionDy);
                Render();
                lastMousePt = currentMousePt;
            };

            /*MouseRightButtonDown += (_, __) =>
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
            };*/

            MouseUp += (_, __) => { panningInProgress = false; };

            MouseWheel += (_, args) =>
            {
                var step = Math.Sign(args.Delta);

                ZoomLevel += step;
                ZoomLevelChanged();
            };

            SetDefaultBtn.Click += (__, ___) =>
            {
                _ = _fractalType switch
                {
                    FractalTypeEnum.MandelbrotSet => IsMandelbrotSet = true,
                    FractalTypeEnum.JuliaSet => IsJuliaSet = true,
                    FractalTypeEnum.BarnsleyFern => IsBarnsleyFern = true,
                    FractalTypeEnum.CollatzConjecture => IsCollatzConjecture = true,
                    _ => throw new ArgumentOutOfRangeException()
                };
            };

            SetRegionBtn.Click += async (_, __) =>
            {
                var setRegionDialog = new SetRegionDialog();
                var result = Convert.ToBoolean(await DialogHost.Show(setRegionDialog, "RootDialog", SetRegionDialogClosing));

                if (!result)
                {
                    return;
                }

                BottomLeft = new Point(setRegionDialog.BottomLeftX, setRegionDialog.BottomLeftY);
                TopRight = new Point(setRegionDialog.TopRightX, setRegionDialog.TopRightY);
                AdjustAspectRatio();
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

            ZoomLevelSlider.ValueChanged += (_, __) => { ZoomLevelChanged(); };
        }

        public static FrozenDictionary<ColourMapEnum, int[]> AvailableColourMaps => new Dictionary<ColourMapEnum, int[]>
        {
            {
                ColourMapEnum.Jet, JetColourMap
            },
            {
                ColourMapEnum.GistStern, GistSternColourMap
            },
            {
                ColourMapEnum.Ocean, OceanColourMap
            },
            {
                ColourMapEnum.Rainbow, RainbowColourMap
            },
            {
                ColourMapEnum.GnuPlot, GnuPlotColourMap
            },
            {
                ColourMapEnum.AfmHot, AfmHotColourMap
            },
            {
                ColourMapEnum.GistHeat, GistHeatColourMap
            },
            {
                ColourMapEnum.ForestGreenWhite, ForestGreenWhiteColourMap
            },
            {
                ColourMapEnum.ForestGreenBlack, ForestGreenBlackColourMap
            },
            {
                ColourMapEnum.Monochrome, MonochromeColourMap
            }
        }.ToFrozenDictionary();

        public Point BottomLeft
        {
            get => _bottomLeft;
            set
            {
                _bottomLeft = value;
                SetStatusBarRightText();
                OnPropertyChanged();
            }
        }

        public Point TopRight
        {
            get => _topRight;
            set
            {
                _topRight = value;
                SetStatusBarRightText();
                OnPropertyChanged();
            }
        }

        public int MaxIterations
        {
            get => _maxIterations;
            set
            {
                _maxIterations = value;
                OnPropertyChanged();
            }
        }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (value > 0)
                {
                    _zoomLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMandelbrotSet
        {
            get => _fractalType == FractalTypeEnum.MandelbrotSet;
            set
            {
                _fractalType = FractalTypeEnum.MandelbrotSet;
                OnPropertyChanged(IS_MANDELBROT_SET_TEXT);
                OnPropertyChanged(IS_JULIA_SET_TEXT);
                OnPropertyChanged(IS_BARNSLEY_FERN_TEXT);
                OnPropertyChanged(IS_COLLATZ_CONJECTURE_TEXT);
                UpdateSelectedFractal();

                ZoomLevel = 1;
                MaxIterations = 120;

                SetDefaultPosition();
                AdjustAspectRatio();

                SelectedColourMap = AvailableColourMaps[ColourMapEnum.Jet];
                Render();
            }
        }

        public bool IsJuliaSet
        {
            get => _fractalType == FractalTypeEnum.JuliaSet;
            set
            {
                _fractalType = FractalTypeEnum.JuliaSet;
                OnPropertyChanged(IS_MANDELBROT_SET_TEXT);
                OnPropertyChanged(IS_JULIA_SET_TEXT);
                OnPropertyChanged(IS_BARNSLEY_FERN_TEXT);
                OnPropertyChanged(IS_COLLATZ_CONJECTURE_TEXT);
                UpdateSelectedFractal();

                ZoomLevel = 1;
                MaxIterations = 4096;

                SetDefaultPosition();
                AdjustAspectRatio();

                SelectedColourMap = AvailableColourMaps[ColourMapEnum.Jet];
                Render();
            }
        }

        public bool IsBarnsleyFern
        {
            get => _fractalType == FractalTypeEnum.BarnsleyFern;
            set
            {
                _fractalType = FractalTypeEnum.BarnsleyFern;
                OnPropertyChanged(IS_MANDELBROT_SET_TEXT);
                OnPropertyChanged(IS_JULIA_SET_TEXT);
                OnPropertyChanged(IS_BARNSLEY_FERN_TEXT);
                OnPropertyChanged(IS_COLLATZ_CONJECTURE_TEXT);
                UpdateSelectedFractal();

                ZoomLevel = 1;
                MaxIterations = 1_000_000;

                SetDefaultPosition();
                AdjustAspectRatio();

                SelectedColourMap = AvailableColourMaps[ColourMapEnum.ForestGreenBlack];
                Render();
            }
        }

        public bool IsCollatzConjecture
        {
            get => _fractalType == FractalTypeEnum.CollatzConjecture;
            set
            {
                _fractalType = FractalTypeEnum.CollatzConjecture;
                OnPropertyChanged(IS_MANDELBROT_SET_TEXT);
                OnPropertyChanged(IS_JULIA_SET_TEXT);
                OnPropertyChanged(IS_BARNSLEY_FERN_TEXT);
                OnPropertyChanged(IS_COLLATZ_CONJECTURE_TEXT);
                UpdateSelectedFractal();

                ZoomLevel = 1;
                MaxIterations = 200;

                SetDefaultPosition();
                AdjustAspectRatio();

                SelectedColourMap = AvailableColourMaps[ColourMapEnum.Jet];
                Render();
            }
        }

        public bool IsGpuDataTypeFloat
        {
            get => !_isGpuDataTypeDouble;
            set
            {
                _isGpuDataTypeDouble = !value;
                OnPropertyChanged();
                UpdateSelectedFractal();
                Render();
            }
        }

        public bool IsGpuDataTypeDouble
        {
            get => _isGpuDataTypeDouble;
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
            get => _selectedColourMap;
            set
            {
                _selectedColourMap = value;
                OnPropertyChanged();
                Render();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private static void SetRegionDialogClosing(object _, DialogClosingEventArgs __)
        {
        }

        private void SetDefaultPosition()
        {
            switch (_fractalType)
            {
                case FractalTypeEnum.MandelbrotSet:
                {
                    BottomLeft = new Point(-2.25d, -1.5d);
                    TopRight = new Point(0.75d, 1.5d);

                    break;
                }

                case FractalTypeEnum.JuliaSet:
                {
                    BottomLeft = new Point(-1.5d, -1.5d);
                    TopRight = new Point(1.5d, 1.5d);

                    break;
                }

                case FractalTypeEnum.BarnsleyFern:
                {
                    BottomLeft = new Point(-3d, -1d);
                    TopRight = new Point(3d, 11d);

                    break;
                }

                case FractalTypeEnum.CollatzConjecture:
                {
                    BottomLeft = new Point(-2.25d, -1.5d);
                    TopRight = new Point(0.75d, 1.5d);

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AdjustAspectRatio()
        {
            if (_fractalImageWidth > _fractalImageHeight)
            {
                var regionWidth = TopRight.X - BottomLeft.X;
                var newRegionWidthDiff = _fractalImageWidth * regionWidth / _fractalImageHeight - regionWidth;
                var halfNewRegionWidthDiff = newRegionWidthDiff / 2;
                BottomLeft = new Point(BottomLeft.X - halfNewRegionWidthDiff, BottomLeft.Y);
                TopRight = new Point(TopRight.X + halfNewRegionWidthDiff, TopRight.Y);
            }
            else if (_fractalImageHeight > _fractalImageWidth)
            {
                var regionHeight = TopRight.Y - BottomLeft.Y;
                var newRegionHeightDiff = _fractalImageHeight * regionHeight / _fractalImageWidth - regionHeight;
                var halfNewRegionHeightDiff = newRegionHeightDiff / 2;
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

        private void UpdateSelectedFractal()
        {
            _selectedFractal = _dict[(_fractalType, _isGpuDataTypeDouble)];
        }

        private void SetStatusBarLeftText(TimeSpan elapsedTime1, TimeSpan elapsedTime2, TimeSpan elapsedTime3)
        {
            StatusBarLeftText.Text = $"{_selectedFractal.GetType().Name}: {elapsedTime1.TotalMilliseconds}ms; {elapsedTime2.TotalMilliseconds}ms; {elapsedTime3.TotalMilliseconds}ms";
        }

        private void SetStatusBarRightText()
        {
            StatusBarRightText.Text = $"Region: ({BottomLeft.X}, {BottomLeft.Y}) ({TopRight.X}, {TopRight.Y})";
        }

        private void ZoomLevelChanged()
        {
            if (!_initDone)
            {
                return;
            }

            if (ZoomLevel == _previousZoomLevel)
            {
                return;
            }

            var diff = ZoomLevel - _previousZoomLevel;

            foreach (var _ in Enumerable.Range(0, Math.Abs(diff)))
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

            if (IsMandelbrotSet)
            {
                MaxIterations += diff * 100;
            }
            else if (IsBarnsleyFern)
            {
                MaxIterations += diff * 500_000;
            }

            Render();

            _previousZoomLevel = ZoomLevel;
        }

        private void Render()
        {
            if (!_initDone)
            {
                return;
            }

            var tuple1 = TimeIt(() => _selectedFractal.CreatePixelArray(
                new Complex(_juliaConstant.X, _juliaConstant.Y),
                new Complex(BottomLeft.X, BottomLeft.Y),
                new Complex(TopRight.X, TopRight.Y),
                _fractalImageWidth,
                _fractalImageHeight,
                MaxIterations));

            var values = tuple1.Result;
            var elapsedTime1 = tuple1.ElapsedTime;

            var tuple2 = TimeIt(() => ValuesToPixels(values, _selectedColourMap));
            var pixels = tuple2.Result;
            var elapsedTime2 = tuple2.ElapsedTime;

            var elapsedTime3 = TimeIt(() =>
            {
                var sourceRect = new Int32Rect(0, 0, _fractalImageWidth, _fractalImageHeight);
                _writeableBitmap.WritePixels(sourceRect, pixels, _writeableBitmap.BackBufferStride, 0);
            });

            SetStatusBarLeftText(elapsedTime1, elapsedTime2, elapsedTime3);
        }

        private static int[] ValuesToPixels(IReadOnlyList<ushort> values, IReadOnlyList<int> colourMap)
        {
            var lastIndex = colourMap.Count - 1;

            var vmin = Convert.ToDouble(values.Min());
            var vmax = Convert.ToDouble(values.Max());

            var divisor = vmax - vmin;

            var cs = new int[values.Count];

            Parallel.For(0, values.Count, i =>
            {
                var p = values[i];
                var v1 = (p - vmin) / divisor;
                var v2 = double.IsNaN(v1) ? 0d : v1;
                var c = colourMap[(int)Math.Floor(v2 * lastIndex)];
                cs[i] = c;
            });

            return cs;
        }

        private static (T Result, TimeSpan ElapsedTime) TimeIt<T>(Func<T> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();

            return (result, stopwatch.Elapsed);
        }

        private static TimeSpan TimeIt(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            return stopwatch.Elapsed;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}