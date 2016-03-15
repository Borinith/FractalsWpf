using System;
using System.Windows.Media;

namespace FractalsWpf
{
    public static class ColorExtensions
    {
        public static int ToInt(this Color c)
        {
            return BitConverter.ToInt32(c.ToByteArray(), 0);
        }

        private static byte[] ToByteArray(this Color c)
        {
            return new[] {c.B, c.G, c.R, (byte) 0};
        }
    }
}
