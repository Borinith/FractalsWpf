//using System.Numerics;
//using MathNet.Numerics;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

                var sourceRect = new Int32Rect(0, 0, 50, 50);
                Array sourceBuffer = Enumerable.Repeat(0x000000FF, _fractalImageWidth * _fractalImageHeight).ToArray();
                _writeableBitmap.WritePixels(sourceRect, sourceBuffer, _writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = _writeableBitmap;
            };

            //var xvalues = Generate.LinearSpaced(11, 0.0, 1.0);
            //var z = new Complex(0, 0);
            //var c = new Complex(0.22, -0.577);
            //z = z*z + c;
            //var m = z.Magnitude;
        }
    }
}
