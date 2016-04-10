using System;
using System.Collections.Generic;

namespace FractalsWpf
{
    public static class ColourMapDataDictionary
    {
        public static Dictionary<string, double[][]> ColourMapData(string name)
        {
            Dictionary<string, double[][]> value;
            return DataDictionary.TryGetValue(name, out value) ? value : null;
        }

        public static Dictionary<string, Func<double, double>> ColourMapData2(string name)
        {
            Dictionary<string, Func<double, double>> value;
            return DataDictionary2.TryGetValue(name, out value) ? value : null;
        }

        private static readonly Dictionary<string, double[][]> JetData = new Dictionary<string, double[][]>
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

        private static readonly Dictionary<string, double[][]> GistSternData = new Dictionary<string, double[][]>
        {
            {
                "red", new[]
                {
                    new[] {0d, 0, 0},
                    new[] {0.0547, 1, 1},
                    new[] {0.250, 0.027, 0.250},
                    new[] {1d, 1, 1}
                }
            },
            {
                "green", new[]
                {
                    new[] {0d, 0, 0},
                    new[] {1d, 0, 0}
                }
            },
            {
                "blue", new[]
                {
                    new[] {0d, 0, 0},
                    new[] {0.5, 1, 1},
                    new[] {0.735, 0, 0},
                    new[] {1d, 0, 0}
                }
            }
        };

        private static readonly Dictionary<string, Dictionary<string, double[][]>> DataDictionary =
            new Dictionary<string, Dictionary<string, double[][]>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"jet", JetData},
                {"gist_stern", GistSternData}
            };

        private static readonly Dictionary<string, Func<double, double>> OceanData =
            new Dictionary<string, Func<double, double>>
            {
                {"red", x => 3 * x - 2},
                {"green", x => Math.Abs((3 * x - 1) / 2)},
                {"blue", x => x}
            };

        private static readonly Dictionary<string, Func<double, double>> RainbowData =
            new Dictionary<string, Func<double, double>>
            {
                {"red", x => Math.Abs(2 * x - 0.5)},
                {"green", x => Math.Sin(x * Math.PI)},
                {"blue", x => Math.Cos(x * Math.PI / 2)}
            };

        private static readonly Dictionary<string, Func<double, double>> GnuPlotData =
            new Dictionary<string, Func<double, double>>
            {
                {"red", Math.Sqrt},
                {"green", x => Math.Pow(x, 3)},
                {"blue", x => Math.Sin(x * 2 * Math.PI)}
            };

        private static readonly Dictionary<string, Dictionary<string, Func<double, double>>> DataDictionary2 =
            new Dictionary<string, Dictionary<string, Func<double, double>>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"ocean", OceanData},
                {"rainbow", RainbowData},
                {"gnuplot", GnuPlotData}
            };
    }
}
