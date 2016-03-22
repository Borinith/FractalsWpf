using System;
using System.Numerics;
using OpenCL;

namespace FractalsWpf
{
    class MandelbrotSetGpu : IFractals, IDisposable
    {
        private readonly OpenCLRunner _runner;

        public MandelbrotSetGpu()
        {
            _runner = new OpenCLRunner("MandelbrotSetCreatePixelArray");
        }

        public int[] CreatePixelArray(
            Complex _,
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
            var realDelta = (maxReal - minReal)/(numWidthDivisions - 1);
            var imaginaryDelta = (maxImaginary - minImaginary)/(numHeightDivisions - 1);
            var numResults = numWidthDivisions * numHeightDivisions;
            var results = new int[numResults];
            const OpenCLMemoryFlags bufferFlags = OpenCLMemoryFlags.WriteOnly | OpenCLMemoryFlags.AllocateHostPointer;
            var bufferCount = new long[] {numResults};
            var bufferElementType = typeof (int);

            using (var resultsBuffer = new OpenCLBuffer(_runner.Context, bufferFlags, bufferElementType, bufferCount))
            {
                _runner.Kernel.SetValueArgument(0, minReal);
                _runner.Kernel.SetValueArgument(1, minImaginary);
                _runner.Kernel.SetValueArgument(2, realDelta);
                _runner.Kernel.SetValueArgument(3, imaginaryDelta);
                _runner.Kernel.SetValueArgument(4, maxIterations);
                _runner.Kernel.SetMemoryArgument(5, resultsBuffer);

                var globalWorkSize = new long[] { numHeightDivisions, numWidthDivisions };
                _runner.CommandQueue.Execute(_runner.Kernel, null, globalWorkSize, null);

                using (var resultsHandle = new PinnedObject(results))
                {
                    _runner.CommandQueue.ReadFromBuffer(resultsBuffer, resultsHandle, true, 0L, numResults);
                }

                _runner.CommandQueue.Finish();

                return results;
            }
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
