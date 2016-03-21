using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenCL;

namespace FractalsWpf
{
    // ReSharper disable once InconsistentNaming
    public class OpenCLRunner : IDisposable
    {
        public OpenCLRunner(string resourceName)
        {
            var platform = OpenCLPlatform.Platforms.FirstOrDefault(p => p.Vendor.Contains("NVIDIA"));
            Context = new OpenCLContext(
                OpenCLDeviceType.Gpu,
                new OpenCLContextPropertyList(platform),
                null,
                IntPtr.Zero);
            var device = Context.Devices.First();
            CommandQueue = new OpenCLCommandQueue(Context, device, OpenCLCommandQueueProperties.None);
            var program = LoadProgram(resourceName);
            Kernel = program.CreateKernel("CreateMandelbrotSetPixelArray");
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

        private OpenCLProgram LoadProgram(string resourceName)
        {
            var source = GetProgramSourceFromResource(Assembly.GetExecutingAssembly(), resourceName);
            var program = new OpenCLProgram(Context, source);

            try
            {
                program.Build(new List<OpenCLDevice> {CommandQueue.Device}, string.Empty, null, IntPtr.Zero);
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
