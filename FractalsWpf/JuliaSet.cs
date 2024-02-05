using FractalsWpf.Interfaces;
using MathNet.Numerics;
using System.Linq;
using System.Numerics;

namespace FractalsWpf
{
    public class JuliaSet : IFractal
    {
        public ushort[] CreatePixelArray(
            Complex c,
            Complex bottomLeft,
            Complex topRight,
            int numPointsWide,
            int numPointsHigh,
            int maxIterations)
        {
            var realValues = Generate.LinearSpaced(numPointsWide, bottomLeft.Real, topRight.Real);
            var imaginaryValues = Generate.LinearSpaced(numPointsHigh, bottomLeft.Imaginary, topRight.Imaginary);

            var pixels =
                from imaginary in imaginaryValues
                from real in realValues
                let pt = new Complex(real, imaginary)
                select BeginsToDivergeAt(c, pt, maxIterations);

            return pixels.ToArray();
        }

        private static ushort BeginsToDivergeAt(Complex c, Complex z, int maxIterations)
        {
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