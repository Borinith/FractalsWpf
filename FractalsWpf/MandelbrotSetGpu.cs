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
            IReadOnlyList<int> colourTable,
            int numWidthDivisions,
            int numHeightDivisions)
        {
            var minReal = (float)Math.Min(c1.Real, c2.Real);
            var maxReal = (float)Math.Max(c1.Real, c2.Real);
            var minImaginary = (float)Math.Min(c1.Imaginary, c2.Imaginary);
            var maxImaginary = (float)Math.Max(c1.Imaginary, c2.Imaginary);
            var realDelta = (maxReal - minReal)/numWidthDivisions;
            var imaginaryDelta = (maxImaginary - minImaginary)/numHeightDivisions;
            var maxIterations = colourTable.Count;

            const OpenCLMemoryFlags flags1 = OpenCLMemoryFlags.ReadOnly | OpenCLMemoryFlags.UseHostPointer;
            const OpenCLMemoryFlags flags2 = OpenCLMemoryFlags.WriteOnly | OpenCLMemoryFlags.AllocateHostPointer;
            var count1 = new long[] { maxIterations };
            var count2 = new long[] { numWidthDivisions * numHeightDivisions };
            var intType = typeof (int);
            var pixels = new int[count2[0]];

            using (var pinnedColourTable = new PinnedObject(colourTable))
            using (var pinnedPixels = new PinnedObject(pixels))
            using (var bufferColourTable = new OpenCLBuffer(_context, flags1, intType, count1, pinnedColourTable))
            using (var bufferPixels = new OpenCLBuffer(_context, flags2, intType, count2))
            {
                _kernel.SetValueArgument(0, minReal);
                _kernel.SetValueArgument(1, minImaginary);
                _kernel.SetValueArgument(2, realDelta);
                _kernel.SetValueArgument(3, imaginaryDelta);
                _kernel.SetValueArgument(4, maxIterations);
                _kernel.SetMemoryArgument(5, bufferColourTable);
                _kernel.SetMemoryArgument(6, bufferPixels);

                var globalWorkSize = new long[] { numHeightDivisions, numWidthDivisions };
                _commandQueue.Execute(_kernel, null, globalWorkSize, null);
                _commandQueue.ReadFromBuffer(bufferPixels, pinnedPixels, true, 0L, pixels.Length);
                _commandQueue.Finish();

                return pixels;
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
