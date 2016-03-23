using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;

namespace FractalsWpf
{
    public class BarnsleyFern : IFractals
    {
        public ushort[] CreatePixelArray(
            Complex _,
            Complex c1,
            Complex c2,
            int maxIterations,
            int numWidthDivisions,
            int numHeightDivisions)
        {
            var minReal = Math.Min(c1.Real, c2.Real);
            var maxReal = Math.Max(c1.Real, c2.Real);
            var minImaginary = Math.Min(c1.Imaginary, c2.Imaginary);
            var maxImaginary = Math.Max(c1.Imaginary, c2.Imaginary);
            var scaleX = numWidthDivisions/(maxReal - minReal);
            var scaleY = numHeightDivisions/(maxImaginary - minImaginary);
            var translateX = minReal;
            var translateY = minImaginary;
            var numResults = numWidthDivisions * numHeightDivisions;
            var results = new ushort[numResults];

            foreach (var pt in Points().Take(maxIterations))
            {
                var transformedX = (pt.X - translateX) * scaleX;
                var transformedY = (pt.Y - translateY) * scaleY;
                var x = (int)Math.Truncate(transformedX);
                var y = (int)Math.Truncate(transformedY);
                results[(numHeightDivisions - y) * numWidthDivisions + x] = 1;
            }

            return results;
        }

        public void Dispose()
        {
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
