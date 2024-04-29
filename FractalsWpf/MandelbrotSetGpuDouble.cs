﻿using FractalsWpf.Interfaces;
using OpenCL;
using System.Numerics;

namespace FractalsWpf
{
    internal class MandelbrotSetGpuDouble : IFractalDisposable
    {
        private readonly OpenCLRunner _runner;
        private OpenCLBuffer? _resultsBuffer;

        public MandelbrotSetGpuDouble()
        {
            _runner = new OpenCLRunner("CreatePixelArrayMandelbrotSetDouble");
        }

        public ushort[] CreatePixelArray(
            Complex _,
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

            _runner.Kernel.SetValueArgument(0, bottomLeft);
            _runner.Kernel.SetValueArgument(1, new Complex(deltaReal, deltaImaginary));
            _runner.Kernel.SetValueArgument(2, maxIterations);
            _runner.Kernel.SetMemoryArgument(3, _resultsBuffer);
            _runner.RunKernelGlobal2D(numPointsWide, numPointsHigh);
            _runner.ReadBuffer(_resultsBuffer!, results);
            //_runner.ReadMappedBuffer(_resultsBuffer!, results);
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