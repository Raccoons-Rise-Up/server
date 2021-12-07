#if Windows
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GameServer.Logging;
using GameServer.Server;
using GameServer.Utilities;

namespace GameServer.Logging
{
    public static class Terminal
    {
        internal const uint ENABLE_QUICK_EDIT = 0x0040;
        internal const uint ENABLE_MOUSE_INPUT = 0x0010;
        internal const int STD_INPUT_HANDLE = -10; // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.

        [DllImport("user32.dll")]
        internal static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        // https://docs.microsoft.com/en-us/windows/console/setconsolemode
        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("Kernel32")]
        internal static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        internal delegate bool EventHandler(CtrlType sig);
        internal static EventHandler _handler;

        internal enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        internal static void DisableConsoleFeatures()
        {

            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                // ERROR: Unable to get console mode.
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;
            consoleMode &= ~ENABLE_MOUSE_INPUT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return;
            }
        }

        internal static void DisableResize() 
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            var MF_BYCOMMAND = 0x00000000;
            var SC_SIZE = 0xF000;

            if (handle != IntPtr.Zero)
            {
                _ = DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }

        internal static void DisableClose() 
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            var MF_BYCOMMAND = 0x00000000;
            var SC_CLOSE = 0xF060;

            if (handle != IntPtr.Zero)
            {
                _ = DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
            }
        }

        internal static void DisableMinimize() 
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            var MF_BYCOMMAND = 0x00000000;
            var SC_MINIMIZE = 0xF020;

            if (handle != IntPtr.Zero)
            {
                _ = DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
            }
        }

        internal static void DisableMaximize() 
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            var MF_BYCOMMAND = 0x00000000;
            var SC_MAXIMIZE = 0xF030;

            if (handle != IntPtr.Zero)
            {
                _ = DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            }
        }

        internal static bool Handler(CtrlType sig)
        {
            Thread.CurrentThread.Name = "CONSOLE"; // Abrubtly exiting seems to clear the current thread name
            ENetServer.SaveAllOnlinePlayersToDatabase();
            Logger.LogRaw("\nExiting application in 3 seconds...");
            Thread.Sleep(3000);

            Environment.Exit(-1);

            return true;
        }

        internal static void DisableAbruptExit() 
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
        }
    }
}
#endif
