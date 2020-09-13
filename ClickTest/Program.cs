using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClickTest {
    static class Program {
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        const int SW_RESTORE = 9;

        [STAThread]
        static void Main(string[] args) {
            while (true) {
                Process[] processes = Process.GetProcessesByName("discord");
                foreach (Process proc in processes) {
                    proc.Refresh();
                    if (IsIconic(proc.MainWindowHandle)) {
                        ShowWindow(proc.MainWindowHandle, SW_RESTORE);
                    }
                    SetForegroundWindow(proc.MainWindowHandle);
                }
                string message;
                if (args.Length == 0)
                    message = "xp farm";
                else
                    message = args[0];
                int delay;
                if (args.Length > 2 || !int.TryParse(args[1], out delay))
                    delay = 60;
                SendKeys.SendWait(message + "{ENTER}");
                Thread.Sleep(delay * 1000);
            }
        }
    }
}