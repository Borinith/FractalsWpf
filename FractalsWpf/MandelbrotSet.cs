using FractalsWpf.Interfaces;
using MathNet.Numerics;
using System.Linq;
using System.Numerics;

namespace FractalsWpf
{
    public class MandelbrotSet : IFractal
    {
        public ushort[] CreatePixelArray(
            Complex _,
            Complex bottomLeft,
            Complex topRight,
            int numPointsWide,
            int numPointsHigh,
            int maxIterations)
        {
            var realValues = Generate.LinearSpaced(numPointsWide, bottomLeft.Real, topRight.Real);
            var imaginaryValues = Generate.LinearSpaced(numPointsHigh, bottomLeft.Imaginary, topRight.Imaginary);

            var results =
                from imaginary in imaginaryValues
                from real in realValues
                let pt = new Complex(real, imaginary)
                select BeginsToDivergeAt(pt, maxIterations);

            return results.ToArray();
        }

        private static ushort BeginsToDivergeAt(Complex c, int maxIterations)
        {
            var z = Complex.Zero;

            foreach (var iterations in Enumerable.Range(0, maxIterations))
            {
                z = z * z + c;

                if (z.Real * z.Real + z.Imaginary * z.Imaginary >= 4d)
                {
                    return (ushort)iterations;
                }
            }

            return (ushort)maxIterations;
        }
    }
}