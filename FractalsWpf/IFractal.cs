using System;
using System.Numerics;

namespace FractalsWpf
{
    public interface IFractal : IDisposable
    {
        ushort[] CreatePixelArray(
            Complex constant,
            Complex bottomLeft,
            Complex topRight,
            int numPointsWide,
            int numPointsHigh,
            int maxIterations);
        // Could add timings parameter:
        // out Tuple<TimeSpan, TimeSpan> timings
    }
}
