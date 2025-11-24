using System;
using System.Text;
using System.Runtime.InteropServices;

namespace DoNotDisturbFix.Library
{
    internal class HexDump
    {
        public static string Dump(IntPtr pBufferToRead, uint nRange, int nIndentCount)
        {
            var hexBuilder = new StringBuilder();
            IntPtr pBaseAddress = IntPtr.Zero;

            for (var idx = 0; idx < nRange; idx++)
            {
                var readByte = Marshal.ReadByte(pBufferToRead, idx);
                hexBuilder.Append(readByte.ToString("X2"));
            }

            return hexBuilder.ToString();
        }
    }
}
