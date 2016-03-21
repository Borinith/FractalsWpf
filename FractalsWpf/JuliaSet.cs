using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;

namespace FractalsWpf
{
    public class JuliaSet : IFractals
    {
        public int[] CreatePixelArray(
            Complex c,
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
            var realValues = Generate.LinearSpaced(numWidthDivisions, minReal, maxReal);
            var imaginaryValues = Generate.LinearSpaced(numHeightDivisions, minImaginary, maxImaginary);

            var pixels =
                from imaginary in imaginaryValues
                from real in realValues
                let pt = new Complex(real, imaginary)
                select BeginsToDivergeAt(c, pt, maxIterations);

            return pixels.ToArray();
        }

        private static int BeginsToDivergeAt(Complex c, Complex z, int maxIterations)
        {
            foreach (var iterations in Enumerable.Range(0, maxIterations))
            {
                z = z * z + c;
                if (z.Real * z.Real + z.Imaginary * z.Imaginary >= 4d) return iterations;
            }

            return maxIterations;
        }
    }
}
