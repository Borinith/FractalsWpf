using System.Numerics;

namespace FractalsWpf
{
    public interface IFractals
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
