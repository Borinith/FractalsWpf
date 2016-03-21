using System;
using System.Collections.Generic;

namespace FractalsWpf
{
    public static class ColourMapDataDictionary
    {
        public static Dictionary<string, double[][]> ColourMapData(string name)
        {
            return DataDictionary[name];
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

        private static readonly Dictionary<string, Dictionary<string, double[][]>> DataDictionary =
            new Dictionary<string, Dictionary<string, double[][]>>(StringComparer.InvariantCultureIgnoreCase)
        {
                { "jet", JetData}
        };
    }
}
