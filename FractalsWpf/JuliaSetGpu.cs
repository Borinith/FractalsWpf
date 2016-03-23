using System;
using System.Numerics;
using OpenCL;

namespace FractalsWpf
{
    class JuliaSetGpu : IFractal
    {
        private readonly OpenCLRunner _runner;
        private OpenCLBuffer _resultsBuffer;

        public JuliaSetGpu()
        {
            _runner = new OpenCLRunner("CreatePixelArrayJuliaSet");
        }

        public ushort[] CreatePixelArray(
            Complex c,
            Complex c1,
            Complex c2,
            int maxIterations,
            int numWidthDivisions,
            int numHeightDivisions)
        {
            var minReal = (float)Math.Min(c1.Real, c2.Real);
            var maxReal = (float)Math.Max(c1.Real, c2.Real);
            var minImaginary = (float)Math.Min(c1.Imaginary, c2.Imaginary);
            var maxImaginary = (float)Math.Max(c1.Imaginary, c2.Imaginary);
            var deltaReal = (maxReal - minReal)/(numWidthDivisions - 1);
            var deltaImaginary = (maxImaginary - minImaginary)/(numHeightDivisions - 1);
            var numResults = numWidthDivisions * numHeightDivisions;
            var results = new ushort[numResults];

            ReallocateResultsBufferIfNecessary(numResults);

            _runner.Kernel.SetValueArgument(0, new Vector2((float)c.Real, (float)c.Imaginary));
            _runner.Kernel.SetValueArgument(1, new Vector2(minReal, minImaginary));
            _runner.Kernel.SetValueArgument(2, new Vector2(deltaReal, deltaImaginary));
            _runner.Kernel.SetValueArgument(3, maxIterations);
            _runner.Kernel.SetMemoryArgument(4, _resultsBuffer);
            _runner.RunKernelGlobal2D(numWidthDivisions, numHeightDivisions);
            _runner.ReadBuffer(_resultsBuffer, results);
            _runner.Finish();

            return results;
        }

        public void Dispose()
        {
            _resultsBuffer?.Dispose();
            _runner?.Dispose();
        }

        private void ReallocateResultsBufferIfNecessary(int numResults)
        {
            if ((_resultsBuffer?.Length ?? 0) == numResults) return;
            _resultsBuffer?.Dispose();
            _resultsBuffer = _runner.CreateWriteOnlyBuffer<ushort>(numResults);
        }
    }
}
