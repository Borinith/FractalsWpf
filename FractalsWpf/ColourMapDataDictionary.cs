using FractalsWpf.Enums;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace FractalsWpf
{
    public static class ColourMapDataDictionary
    {
        private static readonly FrozenDictionary<ColourEnum, double[][]> JetData =
            new Dictionary<ColourEnum, double[][]>
            {
                {
                    ColourEnum.Red, new[]
                    {
                        new[] { 0d, 0, 0 },
                        new[] { 0.35, 0, 0 },
                        new[] { 0.66, 1, 1 },
                        new[] { 0.89, 1, 1 },
                        new[] { 1, 0.5, 0.5 }
                    }
                },
                {
                    ColourEnum.Green, new[]
                    {
                        new[] { 0d, 0, 0 },
                        new[] { 0.125, 0, 0 },
                        new[] { 0.375, 1, 1 },
                        new[] { 0.64, 1, 1 },
                        new[] { 0.91, 0, 0 },
                        new[] { 1d, 0, 0 }
                    }
                },
                {
                    ColourEnum.Blue, new[]
                    {
                        new[] { 0, 0.5, 0.5 },
                        new[] { 0.11, 1, 1 },
                        new[] { 0.34, 1, 1 },
                        new[] { 0.65, 0, 0 },
                        new[] { 1d, 0, 0 }
                    }
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourEnum, double[][]> GistSternData =
            new Dictionary<ColourEnum, double[][]>
            {
                {
                    ColourEnum.Red, new[]
                    {
                        new[] { 0d, 0, 0 },
                        new[] { 0.0547, 1, 1 },
                        new[] { 0.250, 0.027, 0.250 },
                        new[] { 1d, 1, 1 }
                    }
                },
                {
                    ColourEnum.Green, new[]
                    {
                        new[] { 0d, 0, 0 },
                        new[] { 1d, 0, 0 }
                    }
                },
                {
                    ColourEnum.Blue, new[]
                    {
                        new[] { 0d, 0, 0 },
                        new[] { 0.5, 1, 1 },
                        new[] { 0.735, 0, 0 },
                        new[] { 1d, 0, 0 }
                    }
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourMapEnum, FrozenDictionary<ColourEnum, double[][]>> DataDictionary =
            new Dictionary<ColourMapEnum, FrozenDictionary<ColourEnum, double[][]>>
            {
                {
                    ColourMapEnum.Jet, JetData
                },
                {
                    ColourMapEnum.GistStern, GistSternData
                }
            }.ToFrozenDictionary();
        
        // ReSharper disable ConvertClosureToMethodGroup
        private static readonly FrozenDictionary<int, Func<double, double>> GnuplotPaletteFunctions =
            new Dictionary<int, Func<double, double>>
            {
                { 0, _ => 0 },
                { 1, _ => 0.5 },
                { 2, _ => 1 },
                { 3, x => x },
                { 4, x => Math.Pow(x, 2) },
                { 5, x => Math.Pow(x, 3) },
                { 6, x => Math.Pow(x, 4) },
                { 7, x => Math.Sqrt(x) },
                { 8, x => Math.Sqrt(Math.Sqrt(x)) },
                { 9, x => Math.Sin(x * Math.PI / 2) },
                { 10, x => Math.Cos(x * Math.PI / 2) },
                { 11, x => Math.Abs(x - 0.5) },
                { 12, x => Math.Pow(2 * x - 1, 2) },
                { 13, x => Math.Sin(x * Math.PI) },
                { 14, x => Math.Abs(Math.Cos(x * Math.PI)) },
                { 15, x => Math.Sin(x * 2 * Math.PI) },
                { 16, x => Math.Cos(x * 2 * Math.PI) },
                { 17, x => Math.Abs(Math.Sin(x * 2 * Math.PI)) },
                { 18, x => Math.Abs(Math.Cos(x * 2 * Math.PI)) },
                { 19, x => Math.Abs(Math.Sin(x * 4 * Math.PI)) },
                { 20, x => Math.Abs(Math.Cos(x * 4 * Math.PI)) },
                { 21, x => 3 * x },
                { 22, x => 3 * x - 1 },
                { 23, x => 3 * x - 2 },
                { 24, x => Math.Abs(3 * x - 1) },
                { 25, x => Math.Abs(3 * x - 2) },
                { 26, x => (3 * x - 1) / 2 },
                { 27, x => (3 * x - 2) / 2 },
                { 28, x => Math.Abs((3 * x - 1) / 2) },
                { 29, x => Math.Abs((3 * x - 2) / 2) },
                { 30, x => x / 0.32 - 0.78125 },
                { 31, x => 2 * x - 0.84 },
                { 32, x => x }, // TODO: 32: lambda x: gfunc32(x),
                { 33, x => Math.Abs(2 * x - 0.5) },
                { 34, x => 2 * x },
                { 35, x => 2 * x - 0.5 },
                { 36, x => 2 * x - 1 }
            }.ToFrozenDictionary();

        // def gfunc32(x):
        //     ret = np.zeros(len(x))
        //     m = (x< 0.25)
        //     ret[m] = 4 * x[m]
        //     m = (x >= 0.25) & (x< 0.92)
        //     ret[m] = -2 * x[m] + 1.84
        //     m = (x >= 0.92)
        //     ret[m] = x[m] / 0.08 - 11.5
        //     return ret

        private static readonly FrozenDictionary<ColourEnum, Func<double, double>> OceanData =
            new Dictionary<ColourEnum, Func<double, double>>
            {
                {
                    ColourEnum.Red, GnuplotPaletteFunctions[23]
                },
                {
                    ColourEnum.Green, GnuplotPaletteFunctions[28]
                },
                {
                    ColourEnum.Blue, GnuplotPaletteFunctions[3]
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourEnum, Func<double, double>> RainbowData =
            new Dictionary<ColourEnum, Func<double, double>>
            {
                {
                    ColourEnum.Red, GnuplotPaletteFunctions[33]
                },
                {
                    ColourEnum.Green, GnuplotPaletteFunctions[13]
                },
                {
                    ColourEnum.Blue, GnuplotPaletteFunctions[10]
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourEnum, Func<double, double>> GnuPlotData =
            new Dictionary<ColourEnum, Func<double, double>>
            {
                {
                    ColourEnum.Red, GnuplotPaletteFunctions[7]
                },
                {
                    ColourEnum.Green, GnuplotPaletteFunctions[5]
                },
                {
                    ColourEnum.Blue, GnuplotPaletteFunctions[15]
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourEnum, Func<double, double>> AfmHotData =
            new Dictionary<ColourEnum, Func<double, double>>
            {
                {
                    ColourEnum.Red, GnuplotPaletteFunctions[34]
                },
                {
                    ColourEnum.Green, GnuplotPaletteFunctions[35]
                },
                {
                    ColourEnum.Blue, GnuplotPaletteFunctions[36]
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourEnum, Func<double, double>> GistHeatData =
            new Dictionary<ColourEnum, Func<double, double>>
            {
                {
                    ColourEnum.Red, x => 1.5 * x
                },
                {
                    ColourEnum.Green, x => 2 * x - 1
                },
                {
                    ColourEnum.Blue, x => 4 * x - 3
                }
            }.ToFrozenDictionary();

        private static readonly FrozenDictionary<ColourMapEnum, FrozenDictionary<ColourEnum, Func<double, double>>> DataDictionary2 =
            new Dictionary<ColourMapEnum, FrozenDictionary<ColourEnum, Func<double, double>>>
            {
                {
                    ColourMapEnum.Ocean, OceanData
                },
                {
                    ColourMapEnum.Rainbow, RainbowData
                },
                {
                    ColourMapEnum.GnuPlot, GnuPlotData
                },
                {
                    ColourMapEnum.AfmHot, AfmHotData
                },
                {
                    ColourMapEnum.GistHeat, GistHeatData
                }
            }.ToFrozenDictionary();

        public static FrozenDictionary<ColourEnum, double[][]>? ColourMapData(ColourMapEnum colourMap)
        {
            return DataDictionary.TryGetValue(colourMap, out var value) ? value : null;
        }

        public static FrozenDictionary<ColourEnum, Func<double, double>>? ColourMapData2(ColourMapEnum colourMap)
        {
            return DataDictionary2.TryGetValue(colourMap, out var value) ? value : null;
        }
    }
}