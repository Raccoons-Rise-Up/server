using System;
using System.Runtime.InteropServices;

namespace GameServer.Logging
{
    public static class Terminal
    {
        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static void DisableResize() 
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

        public static void DisableClose() 
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

        public static void DisableMinimize() 
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

        public static void DisableMaximize() 
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
