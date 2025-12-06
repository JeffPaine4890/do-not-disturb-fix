using DoNotDisturbFix.Library;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DoNotDisturbFix
{
    class DoNotDisturbFix
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        const ulong WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED = 0x0D83063EA3BF5035UL;

        enum DoNotDisturbController : byte
        {
            Windows = 0,
            Program = 255,
        }

        class DoNotDisturbState
        {
            public const byte Off = 0;
            public const byte FullscreenApp = 2;

            private byte state = 0;
            private DoNotDisturbController controller = DoNotDisturbController.Windows;

            public byte GetState()
            {
                return state;
            }

            public void SetState(byte state)
            {
                this.state = state;
            }

            public DoNotDisturbController GetController()
            {
                return controller;
            }

            public void SetController(DoNotDisturbController controller)
            {
                this.controller = controller;
            }
        }

        static void Main(string[] args)
        {
            DoNotDisturbState lastState = CurrentDoNotDisturbState();
            DoNotDisturbState currentState;

            int delayInMilliseconds = 100;

            try
            {
                delayInMilliseconds = int.Parse(args[0]);
            }
            catch
            {
            }

            if (lastState.GetState() == DoNotDisturbState.FullscreenApp)
            {
                lastState.SetState(DoNotDisturbState.Off);
            }

            while (true)
            {
                currentState = CurrentDoNotDisturbState();
                if (currentState.GetState() == DoNotDisturbState.FullscreenApp && IsUserOnDesktop())
                {
                    SetDoNotDisturbState(lastState.GetState());
                }
                else if (currentState.GetController() == DoNotDisturbController.Program && !IsUserOnDesktop())
                {
                    SetDoNotDisturbState(DoNotDisturbState.FullscreenApp);
                }
                else if (currentState.GetState() != DoNotDisturbState.FullscreenApp)
                {
                    lastState = currentState;
                }
                if (delayInMilliseconds > 0)
                {
                    Thread.Sleep(delayInMilliseconds);
                }
            }
        }
        static bool IsUserOnDesktop()
        {
            Process[] processList = Process.GetProcesses();
            IntPtr foregroundWindow = GetForegroundWindow();
            foreach (Process process in processList)
            {
                if (process.MainWindowHandle == foregroundWindow)
                {
                    return false;
                }
            }
            return true;
        }

        static DoNotDisturbState CurrentDoNotDisturbState()
        {
            DoNotDisturbState returnState = new DoNotDisturbState();

            if (Helpers.ReadWnfData(
                WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED,
                out int _,
                out IntPtr pInfoBuffer,
                out uint nInfoLength))
            {
                if (pInfoBuffer != IntPtr.Zero)
                {
                    string currentState = HexDump.Dump(pInfoBuffer, nInfoLength);
                    Marshal.FreeHGlobal(pInfoBuffer);
                    returnState.SetState(byte.Parse(currentState.Substring(0, 2), System.Globalization.NumberStyles.HexNumber));
                    returnState.SetController((DoNotDisturbController)byte.Parse(currentState.Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                }
            }

            return returnState;
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

        static void SetDoNotDisturbState(byte state)
        {
            IntPtr pDataBuffer;
            byte[] dataBytes;

            string hexString = state.ToString("X2") + "0000FF";

            dataBytes = HexStringToBytes(hexString);

            pDataBuffer = Marshal.AllocHGlobal(dataBytes.Length);
            Marshal.Copy(dataBytes, 0, pDataBuffer, dataBytes.Length);

            Helpers.WriteWnfData(WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED, pDataBuffer, dataBytes.Length);

            Marshal.FreeHGlobal(pDataBuffer);
        }
    }
}
