using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;

namespace FractalsWpf
{
    public class FractalsRegular : IFractals
    {
        public int[] CreatePixelArray(
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
                let c = new Complex(real, imaginary)
                let i = CalculateMandelbrotDivergenceIteration(c, maxIterations)
                select colourTable[i];

            return pixels.ToArray();
        }

        private static int CalculateMandelbrotDivergenceIteration(Complex c, int maxIterations)
        {
            var z = new Complex(0d, 0d);

            foreach (var iterations in Enumerable.Range(0, maxIterations))
            {
                z = z * z + c;
                if (z.Real * z.Real + z.Imaginary * z.Imaginary >= 4d) return iterations;
            }

            return maxIterations;
        }
    }
}
