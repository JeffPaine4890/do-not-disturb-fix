using System;
using System.Text;
using System.Runtime.InteropServices;

namespace DoNotDisturbFix.Library
{
    internal class HexDump
    {
        public static string Dump(IntPtr pBufferToRead, uint nRange)
        {
            var hexBuilder = new StringBuilder();

            for (var idx = 0; idx < nRange; idx++)
            {
                var readByte = Marshal.ReadByte(pBufferToRead, idx);
                hexBuilder.Append(readByte.ToString("X2"));
            }

            return hexBuilder.ToString();
        }
    }
}
