using System.Collections.Generic;
using System.Numerics;

namespace FractalsWpf
{
    public interface IFractals
    {
        int[] CreatePixelArray(
            Complex constant,
            Complex c1,
            Complex c2,
            IReadOnlyList<int> colourTable,
            int numWidthDivisions,
            int numHeightDivisions);
    }
}
