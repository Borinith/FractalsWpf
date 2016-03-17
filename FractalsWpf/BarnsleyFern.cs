using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FractalsWpf
{
    public static class BarnsleyFern
    {
        public static int[] CreatePixelArray(int width, int height, int colour, int maxIterations)
        {
            var pixels = Enumerable.Repeat(0x00FFFFFF, width*height).ToArray();

            // −2.1820 < x < 2.6558 and 0 ≤ y < 9.9983
            const double minX = -3.0;
            const double maxX = 3.0;
            const double minY = -1.0;
            const double maxY = 11.0;
            var scaleX = width/(maxX - minX);
            var scaleY = height/(maxY - minY);
            const double translateX = minX;
            const double translateY = minY;

            foreach (var pt in Points().Take(maxIterations))
            {
                var transformedX = (pt.X - translateX)*scaleX;
                var transformedY = (pt.Y - translateY)*scaleY;
                var x = (int)Math.Truncate(transformedX);
                var y = (int)Math.Truncate(transformedY);
                pixels[(height - y)*width + x] = colour;
            }

            return pixels;
        }

        // ReSharper disable once FunctionNeverReturns
        private static IEnumerable<Point> Points()
        {
            var random = new Random((int)DateTime.UtcNow.Ticks);

            var pt = new Point();
            yield return pt;

            for (;;)
            {
                var xn = pt.X;
                var yn = pt.Y;
                var r = random.Next(1, 101);

                if (r == 1)
                {
                    // xn + 1 = 0
                    // yn + 1 = 0.16 yn
                    pt = new Point(0, 0.16*yn);
                }
                else if (r <= 1 + 85)
                {
                    // xn + 1 = 0.85 xn + 0.04 yn
                    // yn + 1 = −0.04 xn + 0.85 yn + 1.6
                    pt = new Point(0.85*xn + 0.04*yn, -0.04*xn + 0.85*yn + 1.6);
                }
                else if (r <= 1 + 85 + 7)
                {
                    // xn + 1 = 0.2 xn − 0.26 yn
                    // yn + 1 = 0.23 xn + 0.22 yn + 1.6
                    pt = new Point(0.2*xn - 0.26*yn, 0.23*xn + 0.22*yn + 1.6);
                }
                else
                {
                    // xn + 1 = −0.15 xn + 0.28 yn
                    // yn + 1 = 0.26 xn + 0.24 yn + 0.44
                    pt = new Point(-0.15*xn + 0.28*yn, 0.26*xn + 0.24*yn + 0.44);
                }

                yield return pt;
            }
        }
    }
}
