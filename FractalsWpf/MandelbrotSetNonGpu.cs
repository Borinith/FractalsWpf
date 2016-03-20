using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;

namespace FractalsWpf
{
    public class MandelbrotSetNonGpu : IFractals
    {
        public int[] CreatePixelArray(
            Complex _,
            Complex c1,
            Complex c2,
            IReadOnlyList<int> colourTable,
            int numWidthDivisions,
            int numHeightDivisions)
        {
            var minReal = Math.Min(c1.Real, c2.Real);
            var maxReal = Math.Max(c1.Real, c2.Real);
            var minImaginary = Math.Min(c1.Imaginary, c2.Imaginary);
            var maxImaginary = Math.Max(c1.Imaginary, c2.Imaginary);
            var realValues = Generate.LinearSpaced(numWidthDivisions, minReal, maxReal);
            var imaginaryValues = Generate.LinearSpaced(numHeightDivisions, minImaginary, maxImaginary);
            var maxIterations = colourTable.Count - 1;

            var pixels =
                from imaginary in imaginaryValues
                from real in realValues
                let pt = new Complex(real, imaginary)
                let iter = BeginsToDivergeAt(pt, maxIterations)
                //select colourTable[iter];
                select iter;

            return pixels.ToArray();
        }

        private static int BeginsToDivergeAt(Complex c, int maxIterations)
        {
            var z = Complex.Zero;

            foreach (var iterations in Enumerable.Range(0, maxIterations))
            {
                z = z * z + c;
                if (z.Real * z.Real + z.Imaginary * z.Imaginary >= 4d) return iterations;
            }

            return maxIterations;
        }
    }
}
