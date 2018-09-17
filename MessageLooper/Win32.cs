using System;
using System.Runtime.InteropServices;

namespace com.yamanobori_old
{
    public static class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

    }
}
