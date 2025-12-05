using System;
using System.Runtime.InteropServices;
using DoNotDisturbFix.Interop;

namespace DoNotDisturbFix.Library
{
    using NTSTATUS = Int32;

    internal class Helpers
    {
        public static bool ReadWnfData(
            ulong stateName,
            out int nChangeStamp,
            out IntPtr pInfoBuffer,
            out uint nInfoLength)
        {
            NTSTATUS ntstatus;
            nInfoLength = 0x1000u;

            do
            {
                pInfoBuffer = Marshal.AllocHGlobal((int)nInfoLength);
                ntstatus = NativeMethods.NtQueryWnfStateData(
                    in stateName,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out nChangeStamp,
                    pInfoBuffer,
                    ref nInfoLength);

                if ((ntstatus != Win32Consts.STATUS_SUCCESS) || (nInfoLength == 0))
                {
                    Marshal.FreeHGlobal(pInfoBuffer);
                    pInfoBuffer = IntPtr.Zero;
                }
            } while (ntstatus == Win32Consts.STATUS_BUFFER_TOO_SMALL);

            return (ntstatus == Win32Consts.STATUS_SUCCESS);
        }


        public static bool WriteWnfData(ulong stateName, IntPtr pDataBuffer, int nDataSize)
        {
            NTSTATUS ntstatus = NativeMethods.NtUpdateWnfStateData(
                in stateName,
                pDataBuffer,
                nDataSize,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                0);

            return (ntstatus == Win32Consts.STATUS_SUCCESS);
        }
    }
}
