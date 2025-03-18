using FractalsWpf.Enums;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalsWpf
{
    public static class ColourMaps
    {
        // http://matplotlib.org/examples/color/colormaps_reference.html
        // https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/_cm.py
        // https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/colors.py

        public static int[] GetColourMap(ColourMapEnum colourMap)
        {
            const int n = 256;

            var values = GetColourMapRgbaValues(colourMap, n);

            return Enumerable.Range(0, n)
                .Select(index =>
                {
                    var r = (int)Math.Floor(values[index][0] * (n - 1));
                    var g = (int)Math.Floor(values[index][1] * (n - 1));
                    var b = (int)Math.Floor(values[index][2] * (n - 1));

                    return (r << 16) | (g << 8) | b;
                }).ToArray();
        }

        public static double[][] GetColourMapRgbaValues(ColourMapEnum colourMap, int n)
        {
            var colourMapData = ColourMapDataDictionary.ColourMapData(colourMap);

            if (colourMapData != null)
            {
                var rs = MakeMappingArray(n, colourMapData[ColourEnum.Red]);
                var gs = MakeMappingArray(n, colourMapData[ColourEnum.Green]);
                var bs = MakeMappingArray(n, colourMapData[ColourEnum.Blue]);

                return RsGsBsToColourValues(n, rs, gs, bs);
            }

            var colourMapData2 = ColourMapDataDictionary.ColourMapData2(colourMap);

            if (colourMapData2 != null)
            {
                var rs = MakeMappingArray2(n, colourMapData2[ColourEnum.Red]);
                var gs = MakeMappingArray2(n, colourMapData2[ColourEnum.Green]);
                var bs = MakeMappingArray2(n, colourMapData2[ColourEnum.Blue]);

                return RsGsBsToColourValues(n, rs, gs, bs);
            }

            throw new InvalidOperationException($"Failed to find a colour map called \"${colourMap}\".");
        }

        private static double[][] RsGsBsToColourValues(
            int n,
            double[] rs,
            double[] gs,
            double[] bs)
        {
            var values = new List<double[]>();

            for (var i = 0; i < n; i++)
            {
                values.Add([rs[i], gs[i], bs[i], 1d]);
            }

            return values.ToArray();
        }

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
            var distance = Enumerable.Range(0, n - 2)
                .Select(i =>
                {
                    var numerator = xind[i + 1] - x[ind[i] - 1];
                    var denominator = x[ind[i]] - x[ind[i] - 1];

                    return numerator / denominator;
                }).ToArray();

            // lut[1:-1] = distance * (y0[ind] - y1[ind - 1]) + y1[ind - 1]
            Enumerable.Range(0, n - 2)
                .ToList()
                .ForEach(i =>
                {
                    lut[i + 1] = distance[i] * (y0[ind[i]] - y1[ind[i] - 1]) + y1[ind[i] - 1];
                });

            // lut[0] = y1[0]
            lut[0] = y1[0];

            // lut[-1] = y0[-1]
            lut[n - 1] = y0.Last();

            // # ensure that the lut is confined to values between 0 and 1 by clipping it
            // return np.clip(lut, 0.0, 1.0)
            return lut.Select(ClipZeroToOne).ToArray();
        }

        private static int[] SearchSorted(double[] arr, double[] vs)
        {
            var result = new int[vs.Length];
            var arrLen = arr.Length;

            for (var i = 0; i < vs.Length; i++)
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

                if (!added)
                {
                    result[i] = arrLen;
                }
            }

            return result;
        }

        private static double[] MakeMappingArray2(int n, Func<double, double> lambda)
        {
            return Generate.LinearSpaced(n, 0d, 1d)
                .Select(lambda)
                .Select(ClipZeroToOne)
                .ToArray();
        }

        private static double ClipZeroToOne(double value)
        {
            return Clip(0d, 1d, value);
        }

        private static double Clip(double min, double max, double value)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}