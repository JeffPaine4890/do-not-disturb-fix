using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace InitialSetup
{
    class InitialSetup
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

        const int GWL_STYLE = -16;
        const int WS_VISIBLE = 0x10000000;
        const int WS_MINIMIZE = 0x20000000;

        static void Main()
        {
            MessageBox.Show("Please close or minimize ALL open windows. The only thing visible should be this message box over your desktop.", "Do Not Disturb Fix - Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            try
            {
                StreamWriter blacklistWriter = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\blacklist.dat", false);
                // check for each window that is visible and not minimized
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

                        // Now that we've got the window title and handle, we'll write them to our blacklist
                        blacklistWriter.WriteLine(Process.GetProcessById((int)processId).ProcessName.ToString());
                        blacklistWriter.WriteLine(windowText);
                    }
                    return true;
                }, IntPtr.Zero);
                blacklistWriter.Close();
            }
            catch
            {
                MessageBox.Show("Error writing blacklist.dat", "Do Not Disturb Fix - Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("blacklist.dat successfully created. You can now run DoNotDisturbFix.exe.", "Do Not Disturb Fix - Initial Setup");
            return;
        }
    }
}

