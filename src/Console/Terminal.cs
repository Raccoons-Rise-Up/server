#if Windows
using System;
using System.Runtime.InteropServices;

namespace GameServer.Logging
{
    public static class Terminal
    {
        private const uint ENABLE_QUICK_EDIT = 0x0040;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const int STD_INPUT_HANDLE = -10; // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        // https://docs.microsoft.com/en-us/windows/console/setconsolemode
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

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
    }
}
#endif
