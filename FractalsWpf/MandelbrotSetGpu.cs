using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using OpenCL;

namespace FractalsWpf
{
    class MandelbrotSetGpu : IFractals, IDisposable
    {
        private readonly OpenCLContext _context;
        private readonly OpenCLCommandQueue _commandQueue;
        private readonly OpenCLKernel _kernel;

        public MandelbrotSetGpu()
        {
            var platform = OpenCLPlatform.Platforms.FirstOrDefault(p => p.Vendor.Contains("NVIDIA"));
            _context = new OpenCLContext(
                OpenCLDeviceType.Gpu,
                new OpenCLContextPropertyList(platform),
                null,
                IntPtr.Zero);
            var device = _context.Devices.First();
            _commandQueue = new OpenCLCommandQueue(_context, device, OpenCLCommandQueueProperties.None);
            var program = LoadProgram(_context, device, "FractalsWpf.CreateMandelbrotSetPixelArray.cl");
            _kernel = program.CreateKernel("CreateMandelbrotSetPixelArray");
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

            using (var resultsBuffer = new OpenCLBuffer(_context, bufferFlags, bufferElementType, bufferCount))
            {
                _kernel.SetValueArgument(0, minReal);
                _kernel.SetValueArgument(1, minImaginary);
                _kernel.SetValueArgument(2, realDelta);
                _kernel.SetValueArgument(3, imaginaryDelta);
                _kernel.SetValueArgument(4, maxIterations);
                _kernel.SetMemoryArgument(5, resultsBuffer);

                var globalWorkSize = new long[] { numHeightDivisions, numWidthDivisions };
                _commandQueue.Execute(_kernel, null, globalWorkSize, null);

                using (var resultsHandle = new PinnedObject(results))
                {
                    _commandQueue.ReadFromBuffer(resultsBuffer, resultsHandle, true, 0L, numResults);
                }

                _commandQueue.Finish();

                return results;
            }
        }

        public void Dispose()
        {
            _kernel?.Dispose();
            _commandQueue?.Dispose();
            _context?.Dispose();
        }

        private static OpenCLProgram LoadProgram(OpenCLContext context, OpenCLDevice device, string resourceName)
        {
            var source = GetProgramSourceFromResource(Assembly.GetExecutingAssembly(), resourceName);
            var program = new OpenCLProgram(context, source);

            try
            {
                program.Build(new List<OpenCLDevice> { device }, string.Empty, null, IntPtr.Zero);
            }
            catch (BuildProgramFailureOpenCLException)
            {
                var buildLog = program.GetBuildLog(device);
                throw new ApplicationException($"Error building program \"{resourceName}\":{Environment.NewLine}{buildLog}");
            }

            return program;
        }

        private static string GetProgramSourceFromResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new ApplicationException($"Failed to load resource {resourceName}");
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
