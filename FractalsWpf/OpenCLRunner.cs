using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenCL;

namespace FractalsWpf
{
    // ReSharper disable once InconsistentNaming
    public class OpenCLRunner : IDisposable
    {
        public OpenCLRunner(string functionName)
        {
            var platform = OpenCLPlatform.Platforms.FirstOrDefault(p => p.Vendor.Contains("NVIDIA"));
            Context = new OpenCLContext(
                OpenCLDeviceType.Gpu,
                new OpenCLContextPropertyList(platform),
                null,
                IntPtr.Zero);
            var device = Context.Devices.First();
            CommandQueue = new OpenCLCommandQueue(Context, device, OpenCLCommandQueueProperties.None);
            var resourceName = $"FractalsWpf.OpenCL.{functionName}.cl";
            var program = LoadProgram(resourceName);
            Kernel = program.CreateKernel(functionName);
        }

        public OpenCLContext Context { get; }
        public OpenCLCommandQueue CommandQueue { get; }
        public OpenCLKernel Kernel { get; }

        public void Dispose()
        {
            Kernel?.Dispose();
            CommandQueue?.Dispose();
            Context?.Dispose();
        }

        public OpenCLBuffer CreateWriteOnlyBuffer<T>(long count)
        {
            const OpenCLMemoryFlags flags = OpenCLMemoryFlags.WriteOnly | OpenCLMemoryFlags.AllocateHostPointer;
            return new OpenCLBuffer(Context, flags, typeof(T), new [] {count});
        }

        public void ReadBuffer<T>(OpenCLBuffer sourceBuffer, T[] destinationArray)
        {
            using (var destinationArrayHandle = new PinnedObject(destinationArray))
            {
                CommandQueue.ReadFromBuffer(
                    sourceBuffer,
                    destinationArrayHandle,
                    true, // blocking
                    0L, // offset
                    destinationArray.Length); // region
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public void ReadMappedBuffer<T>(OpenCLBuffer sourceBuffer, T[] destinationArray)
        {
            using (var destinationArrayHandle = new PinnedObject(destinationArray))
            {
                var mappedPtr = MapBufferForReading(sourceBuffer);
                var cb = (uint)(destinationArray.Length * Marshal.SizeOf(typeof(T)));
                CopyMemory(destinationArrayHandle, mappedPtr, cb);
                UnmapBuffer(sourceBuffer, ref mappedPtr);
            }
        }

        private IntPtr MapBufferForReading(OpenCLBufferBase buffer)
        {
            return CommandQueue.Map(
                buffer,
                true, // blocking
                OpenCLMemoryMappingFlags.Read,
                0, // offset
                buffer.Length);
        }

        private void UnmapBuffer(OpenCLBuffer buffer, ref IntPtr mappedPtr)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (mappedPtr == IntPtr.Zero) return;
            CommandQueue.Unmap(buffer, ref mappedPtr);
        }

        public void RunKernelGlobal2D(int globalWorkSize0, int globalWorkSize1)
        {
            var globalWorkSize = new long[] {globalWorkSize0, globalWorkSize1};
            CommandQueue.Execute(
                Kernel,
                null, // globalWorkOffset
                globalWorkSize,
                null); // localWorkSize
        }

        public void Finish()
        {
            CommandQueue.Finish();
        }

        private OpenCLProgram LoadProgram(string resourceName)
        {
            var source = GetProgramSourceFromResource(Assembly.GetExecutingAssembly(), resourceName);
            var program = new OpenCLProgram(Context, source);

            try
            {
                var optionsList = new[]
                {
                    "-cl-fast-relaxed-math",
                    "-cl-mad-enable",
                    "-cl-no-signed-zeros",
                    "-cl-strict-aliasing",
                    "-Werror"
                };
                var options = string.Join(" ", optionsList);
                program.Build(new List<OpenCLDevice> {CommandQueue.Device}, options, null, IntPtr.Zero);
            }
            catch (BuildProgramFailureOpenCLException)
            {
                var buildLog = program.GetBuildLog(CommandQueue.Device);
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
