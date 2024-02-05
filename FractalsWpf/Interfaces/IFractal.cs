using System.Numerics;

namespace FractalsWpf.Interfaces
{
    public interface IFractal
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