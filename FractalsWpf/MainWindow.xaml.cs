﻿using System;
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
        private bool _isGpuDataTypeDouble;
        private IFractal _selectedFractal;
        private int _fractalImageWidth;
        private int _fractalImageHeight;
        private WriteableBitmap _writeableBitmap;
        private int _maxIterations;
        private int _zoomLevel;
        private Point _bottomLeft;
        private Point _topRight;
        private bool _initDone;

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
                IsGpuDataTypeDouble = false;

                _initDone = true;

                Render();
            };

            ZoomLevelSlider.ValueChanged += (_, args) =>
            {
                if (!_initDone) return;

                var diff = args.NewValue - args.OldValue;

                foreach (var idx in Enumerable.Range(0, (int)Math.Abs(diff)))
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

            Closed += (_, __) =>
            {
                _selectedFractal.Dispose();
            };
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

        private void UpdateSelectedFractal()
        {
            _selectedFractal = _isGpuDataTypeDouble ? _mandelbrotSetGpuDouble : _mandelbrotSetGpuFloat;
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
                new Complex(-0.35, 0.65),
                new Complex(BottomLeft.X, BottomLeft.Y), 
                new Complex(TopRight.X, TopRight.Y), 
                _fractalImageWidth,
                _fractalImageHeight,
                MaxIterations));
            var values = tuple1.Item1;
            var elapsedTime1 = tuple1.Item2;

            var tuple2 = TimeIt(() => ValuesToPixels(values, ColourMap));
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
