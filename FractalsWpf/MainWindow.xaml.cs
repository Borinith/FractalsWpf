using System;
using System.Collections.Generic;
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

                var bottomLeft = new Complex(-2d, -2d);
                var topRight = new Complex(2d, 2d);

                //var bottomLeft = new Complex(-2.25d, -1.5d);
                //var topRight = new Complex(0.75d, 1.5d);

                //var bottomLeft = new Complex(-1.5d, -0.5d);
                //var topRight = new Complex(-0.5d, 0.5d);

                //var bottomLeft = new Complex(-0.0d, -0.9d);
                //var topRight = new Complex(0.6d, -0.3d);

                //var bottomLeft = new Complex(-0.22d, -0.70d);
                //var topRight = new Complex(-0.21d, -0.69d);

                const int maxIterations = 120;
                var colourTable = CreateColourTable(maxIterations);

                //IFractals fractals = new MandelbrotSetNonGpu();
                //IFractals fractals = new MandelbrotSetGpu();
                IFractals fractals = new JuliaSetNonGpu();

                var iters = fractals.CreatePixelArray(
                    new Complex(-0.35, 0.65), 
                    bottomLeft,
                    topRight,
                    colourTable,
                    fractalImageWidth,
                    fractalImageHeight);

                var pixels = ItersToPixels(iters);

                //var pixels = BarnsleyFern.CreatePixelArray(
                //    fractalImageWidth,
                //    fractalImageHeight,
                //    Colors.ForestGreen.ToInt(),
                //    10000000);

                var sourceRect = new Int32Rect(0, 0, fractalImageWidth, fractalImageHeight);
                writeableBitmap.WritePixels(sourceRect, pixels, writeableBitmap.BackBufferStride, 0);

                FractalImage.Source = writeableBitmap;
            };
        }

        private static int[] CreateColourTable(int maxIterations)
        {
            var startColour = Colors.Red;
            var stopColour = Colors.Violet;
            var start = startColour.ToInt();
            var stop = stopColour.ToInt();
            return Generate.LinearSpacedMap(maxIterations, start, stop, Convert.ToInt32);
        }

        private static int[] ItersToPixels(int[] iters)
        {
            var vmin = (double)iters.Min();
            var vmax = (double)iters.Max();
            var divisor = vmax - vmin;
            var normalisedIters = iters.Select(p => (p - vmin) / divisor).ToArray();

            var cmap = Jet(256);
            return normalisedIters.Select(p => cmap[(int)Math.Floor(p * 255)]).ToArray();
        }

        private static int[] Jet(int n)
        {
            var jetData = new Dictionary<string, double[][]>
            {
                {
                    "red", new[]
                    {
                        new[] {0d, 0, 0},
                        new[] {0.35, 0, 0},
                        new[] {0.66, 1, 1},
                        new[] {0.89, 1, 1},
                        new[] {1, 0.5, 0.5}
                    }
                },
                {
                    "green", new[]
                    {
                        new[] {0d, 0, 0},
                        new[] {0.125, 0, 0},
                        new[] {0.375, 1, 1},
                        new[] {0.64, 1, 1},
                        new[] {0.91, 0, 0},
                        new[] {1d, 0, 0}
                    }
                },
                {
                    "blue", new[]
                    {
                        new[] {0, 0.5, 0.5},
                        new[] {0.11, 1, 1},
                        new[] {0.34, 1, 1},
                        new[] {0.65, 0, 0},
                        new[] {1d, 0, 0}
                    }
                }
            };

            var rs = MakeMappingArray(n, jetData["red"]);
            var gs = MakeMappingArray(n, jetData["green"]);
            var bs = MakeMappingArray(n, jetData["blue"]);

            return Enumerable.Range(0, n)
                .Select(index =>
                {
                    var r = (int)Math.Floor(rs[index] * (n - 1));
                    var g = (int)Math.Floor(gs[index] * (n - 1));
                    var b = (int)Math.Floor(bs[index] * (n - 1));
                    return r << 16 | g << 8 | b;
                }).ToArray();
        }

        // http://matplotlib.org/examples/color/colormaps_reference.html
        // https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/_cm.py
        // https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/colors.py
        private static double[] MakeMappingArray(int n, double[][] adata)
        {
            var x = adata.Select(e => e[0]).ToArray();
            var y0 = adata.Select(e => e[1]).ToArray();
            var y1 = adata.Select(e => e[2]).ToArray();

            // # begin generation of lookup table
            // x = x * (N - 1)
            x = x.Select(v => v * (n - 1)).ToArray();

            // lut = np.zeros((N,), np.float)
            var lut = new double[n];

            // xind = (N - 1) * np.linspace(0, 1, N) ** gamma
            var xind = Enumerable.Range(0, n).ToArray();

            // ind = np.searchsorted(x, xind)[1:-1]
            var ind = SearchSorted(x, xind.Select(v => (double)v).ToArray());
            ind = ind.Skip(1).Take(ind.Length - 2).ToArray();

            // distance = (xind[1:-1] - x[ind - 1]) / (x[ind] - x[ind - 1])
            // [ 0.19047619  0.38095238  0.57142857  0.76190476  0.95238095  0.16129032
            //   0.37634409  0.59139785  0.80645161  0.02898551  0.31884058  0.60869565
            //   0.89855072  0.39393939]
            var distance = Enumerable.Range(0, n - 2)
                .Select(i =>
                {
                    var numerator = xind[i + 1] - x[ind[i] - 1];
                    var denominator = x[ind[i]] - x[ind[i] - 1];
                    return numerator/denominator;
                }).ToArray();

            // lut[1:-1] = distance * (y0[ind] - y1[ind - 1]) + y1[ind - 1]
            Enumerable.Range(0, n - 2).ToList().ForEach(i =>
            {
                lut[i + 1] = distance[i] * (y0[ind[i]] - y1[ind[i] - 1]) + y1[ind[i] - 1];
            });

            // lut[0] = y1[0]
            lut[0] = y1[0];

            // lut[-1] = y0[-1]
            lut[n - 1] = y0.Last();

            // # ensure that the lut is confined to values between 0 and 1 by clipping it
            // return np.clip(lut, 0.0, 1.0)

            // array([0.        , 0.        , 0.        , 0.        , 0.        ,
            //        0.        , 0.16129032, 0.37634409, 0.59139785, 0.80645161,
            //        1.        , 1.        , 1.        , 1.        , 0.8030303,
            //        0.5])
            return lut;
        }

        private static int[] SearchSorted(IReadOnlyList<double> arr, IReadOnlyList<double> vs)
        {
            var result = new int[vs.Count];
            var arrLen = arr.Count;
            for (var i = 0; i < vs.Count; i++)
            {
                var v = vs[i];
                var added = false;
                for (var j = 0; j < arrLen; j++)
                {
                    if (v <= arr[j])
                    {
                        result[i] = j;
                        added = true;
                        break;
                    }
                }
                if (!added) result[i] = arrLen;
            }
            return result;
        }
    }
}
