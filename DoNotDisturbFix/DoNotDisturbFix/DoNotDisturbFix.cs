using DoNotDisturbFix.Library;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DoNotDisturbFix
{
    class DoNotDisturbFix
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        const ulong WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED = 0x0D83063EA3BF5035UL;

        // This is not a Windows property, rather we're taking advantage of the fact that only the first byte of the WNF_SHEL_QUIET_MOMENT_SHELL_MODE_CHANGED property actually controls anything
        // So if we set the last byte to 0xFF (which Windows will change back to 0x00 whenever it changes the property), we can keep track of who changed the property, us or Windows
        enum DoNotDisturbController : byte
        {
            Windows = 0x00,
            Program = 0xFF,
        }

        class DoNotDisturbState
        {
            public const byte Off = 0x00;
            public const byte Game = 0x01;
            public const byte FullscreenApp = 0x02;

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

            IntPtr lastGameWindow = IntPtr.Zero;

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

                // If Do Not Disturb is set due to a fullscreen app, but the user is on the desktop, revert to the last known Do Not Disturb state
                if (currentState.GetState() == DoNotDisturbState.FullscreenApp && IsUserOnDesktop())
                {
                    SetDoNotDisturbState(lastState.GetState());
                }
                // If Windows switched to Do Not Disturb mode due to a game, we need to keep track of the window handle that last triggered that
                else if (currentState.GetState() == DoNotDisturbState.Game && currentState.GetController() == DoNotDisturbController.Windows)
                {
                    lastGameWindow = GetForegroundWindow();
                }
                // If Do Not Disturb is set due to a game, but the user is on the desktop, turn Do Not Disturb off
                // The main reason this would happen is if the user was playing a game, then switched to a fullscreen app, the game closed while the user was using the
                // fullscreen app, then we closed the fullscreen app
                // In this scenario Do Not Disturb Fix would have reverted to the last known Do Not Disturb state (game) even though the user is no longer playing a game
                // We also have to check to make sure the foreground window is not the last known game window, because IsUserOnDesktop will return true even in a game
                else if (currentState.GetState() == DoNotDisturbState.Game && GetForegroundWindow() != lastGameWindow && IsUserOnDesktop())
                {
                    SetDoNotDisturbState(DoNotDisturbState.Off);
                }
                // If we set Do Not Disturb to off, but the user is no longer on the desktop, then that means we should turn Do Not Disturb on due to a fullscreen program
                // This covers cases such as opening the NVIDIA Overlay; this is supposed to be considered a fullscreen app but since it's not actually a new window,
                // Windows won't set Do Not Disturb mode again assuming it's already been set
                else if (currentState.GetState() == DoNotDisturbState.Off && currentState.GetController() == DoNotDisturbController.Program && !IsUserOnDesktop())
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
            while (true)
            {
                Process[] processList = Process.GetProcesses();
                IntPtr foregroundWindow = GetForegroundWindow();

                if (foregroundWindow == IntPtr.Zero)
                {
                    Thread.Sleep(1);
                    continue;
                }

                GetWindowThreadProcessId(foregroundWindow, out uint processId);
                if (processId == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                foreach (Process process in processList)
                {
                    if (process.MainWindowHandle == foregroundWindow)
                    {
                        return false;
                    }
                }

                return true;
            }
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
