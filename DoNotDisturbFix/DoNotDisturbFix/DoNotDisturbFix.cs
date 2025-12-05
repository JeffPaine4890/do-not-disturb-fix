using DoNotDisturbFix.Library;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DoNotDisturbFix
{
    class DoNotDisturbFix
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        const ulong WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED = 0x0D83063EA3BF5035UL;
        const int GWL_STYLE = -16;
        const int WS_VISIBLE = 0x10000000;
        const int WS_MINIMIZE = 0x20000000;
        static void Main(string[] args)
        {
            // if WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED == 02 00 00 00
            // and there's no windows open
            // set it to what it was before

            string lastState = CurrentDoNotDisturbState();
            string currentState;
            string[] blacklist;

            try
            {
                blacklist = File.ReadAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "blacklist.dat");
            }
            catch
            {
                MessageBox.Show("blacklist.dat not found, please run InitialSetup.exe first");
                return;
            }

            int delayInMilliseconds = 100;

            try
            {
                delayInMilliseconds = int.Parse(args[0]);
            }
            catch
            {
            }

            if (lastState == "02000000")
            {
                lastState = "00000000";
            }

            while (true)
            {
                currentState = CurrentDoNotDisturbState();
                if (currentState == "02000000")
                {
                    // check for each window that is visible and not minimized
                    bool revertDoNotDisturb = true;
                    EnumWindows((hWnd, lParam) =>
                    {
                        int windowStatus = GetWindowLong(hWnd, GWL_STYLE);
                        // check if the window is visible
                        if ((windowStatus & WS_VISIBLE) == WS_VISIBLE && (windowStatus & WS_MINIMIZE) != WS_MINIMIZE)
                        {
                            StringBuilder windowText = new StringBuilder(256);
                            GetWindowText(hWnd, windowText, windowText.Capacity);

                            uint processId;
                            GetWindowThreadProcessId(hWnd, out processId);
                            Console.WriteLine(Process.GetProcessById((int)processId).ProcessName.ToString());
                            Console.WriteLine(windowText);

                            // Now that we've got the window title and handle, we can check it against our blacklist
                            // If this window shows up in our blacklist, then we'll pretend it doesn't exist
                            for (int i = 0; i < blacklist.Length; i += 2)
                            {
                                if (blacklist[i] == "ApplicationFrameHost" || blacklist[i] == Process.GetProcessById((int)processId).ProcessName.ToString() && blacklist[i + 1] == windowText.ToString())
                                {
                                    return true;
                                }
                            }

                            // If we come across a window that's NOT in our blacklist, then we'll assume Do Not Disturb was legitimately activated
                            revertDoNotDisturb = false;
                            return false;
                        }
                        return true;
                    }, IntPtr.Zero);
                    if (revertDoNotDisturb)
                    {
                        SetDoNotDisturbState(lastState);
                    }
                }
                else
                {
                    lastState = currentState;
                }
                if (delayInMilliseconds > 0)
                {
                    Thread.Sleep(delayInMilliseconds);
                }
            }
        }

        static string CurrentDoNotDisturbState()
        {
            if (Helpers.ReadWnfData(
                WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED,
                out int _,
                out IntPtr pInfoBuffer,
                out uint nInfoLength))
            {
                if (pInfoBuffer != IntPtr.Zero)
                {
                    string currentState = HexDump.Dump(pInfoBuffer, nInfoLength, 0);
                    Marshal.FreeHGlobal(pInfoBuffer);
                    return currentState;
                }
            }

            return "";
        }

        static byte[] HexStringToBytes(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                returnBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return returnBytes;
        }

        static void SetDoNotDisturbState(string hexString)
        {
            IntPtr pDataBuffer;
            byte[] dataBytes;

            dataBytes = HexStringToBytes(hexString);

            pDataBuffer = Marshal.AllocHGlobal(dataBytes.Length);
            Marshal.Copy(dataBytes, 0, pDataBuffer, dataBytes.Length);

            Helpers.WriteWnfData(WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED, pDataBuffer, dataBytes.Length);

            Marshal.FreeHGlobal(pDataBuffer);
        }
    }
}
