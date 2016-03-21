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

        public static int[] GetColourMap(string name)
        {
            const int n = 256;
            var colourMapData = ColourMapDataDictionary.ColourMapData(name);

            var rs = MakeMappingArray(n, colourMapData["red"]);
            var gs = MakeMappingArray(n, colourMapData["green"]);
            var bs = MakeMappingArray(n, colourMapData["blue"]);

            return Enumerable.Range(0, n)
                .Select(index =>
                {
                    var r = (int)Math.Floor(rs[index] * (n - 1));
                    var g = (int)Math.Floor(gs[index] * (n - 1));
                    var b = (int)Math.Floor(bs[index] * (n - 1));
                    return r << 16 | g << 8 | b;
                }).ToArray();
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
