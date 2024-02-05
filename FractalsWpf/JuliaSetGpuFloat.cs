using FractalsWpf.Interfaces;
using OpenCL;
using System.Numerics;

namespace FractalsWpf
{
    internal class JuliaSetGpuFloat : IFractalDisposable
    {
        private readonly OpenCLRunner _runner;
        private OpenCLBuffer? _resultsBuffer;

        public JuliaSetGpuFloat()
        {
            _runner = new OpenCLRunner("CreatePixelArrayJuliaSetFloat");
        }

        public ushort[] CreatePixelArray(
            Complex c,
            Complex bottomLeft,
            Complex topRight,
            int numPointsWide,
            int numPointsHigh,
            int maxIterations)
        {
            var deltaReal = (topRight.Real - bottomLeft.Real) / (numPointsWide - 1);
            var deltaImaginary = (topRight.Imaginary - bottomLeft.Imaginary) / (numPointsHigh - 1);
            var numResults = numPointsWide * numPointsHigh;
            var results = new ushort[numResults];

            ReallocateResultsBufferIfNecessary(numResults);

            _runner.Kernel.SetValueArgument(0, new Vector2((float)c.Real, (float)c.Imaginary));
            _runner.Kernel.SetValueArgument(1, new Vector2((float)bottomLeft.Real, (float)bottomLeft.Imaginary));
            _runner.Kernel.SetValueArgument(2, new Vector2((float)deltaReal, (float)deltaImaginary));
            _runner.Kernel.SetValueArgument(3, maxIterations);
            _runner.Kernel.SetMemoryArgument(4, _resultsBuffer);
            _runner.RunKernelGlobal2D(numPointsWide, numPointsHigh);
            _runner.ReadBuffer(_resultsBuffer!, results);
            _runner.Finish();

            return results;
        }

        public void Dispose()
        {
            _resultsBuffer?.Dispose();
            _runner.Dispose();
        }

        private void ReallocateResultsBufferIfNecessary(int numResults)
        {
            if ((_resultsBuffer?.Length ?? 0) == numResults)
            {
                return;
            }

            _resultsBuffer?.Dispose();
            _resultsBuffer = _runner.CreateWriteOnlyBuffer<ushort>(numResults);
        }
    }
}