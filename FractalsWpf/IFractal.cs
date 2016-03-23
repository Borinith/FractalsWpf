using System;
using System.Numerics;

namespace FractalsWpf
{
    public interface IFractal : IDisposable
    {
        ushort[] CreatePixelArray(
            Complex constant,
            Complex c1,
            Complex c2,
            int maxIterations,
            int numWidthDivisions,
            int numHeightDivisions);
    }
}
