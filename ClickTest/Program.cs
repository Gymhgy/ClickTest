using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DiscordSpammer {
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

            string message;
            if (args.Length == 0)
                message = "** **";
            else
                message = args[0];
            int delay = 60;

            if (args.Length > 1)
                int.TryParse(args[1], out delay);

            int messages = 0;



            DateTime lastSend = DateTime.Now;

            Console.WriteLine("Launching program with message '{0}' and {1} sec delay", message, delay);

            new Thread(() => {
                Thread.CurrentThread.IsBackground = true;
                while (true) {
                    Process[] processes = Process.GetProcessesByName("discord");
                    //Check if discord is open
                    if (processes.Length == 0) {
                        Console.WriteLine("\nDiscord is not open. Terminating.");
                        return;
                    }
                    //Set discord to active application
                    //If it's minimized, open it
                    foreach (Process proc in processes) {
                        proc.Refresh();
                        if (IsIconic(proc.MainWindowHandle)) {
                            ShowWindow(proc.MainWindowHandle, SW_RESTORE);
                        }
                        SetForegroundWindow(proc.MainWindowHandle);
                    }


                    SendKeys.SendWait(message + "{ENTER}");
                    lastSend = DateTime.Now;
                    Thread.Sleep(delay * 1000);
                }
            }).Start();

            while (true) {
                var input = Console.ReadLine();
                if (input.Equals("stop", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("{0} messages were sent over {1} seconds. Exiting...", messages, delay);
                    Environment.Exit(Environment.ExitCode);
                }
                if (input.StartsWith("message", StringComparison.OrdinalIgnoreCase)) {
                    var split = input.Split(new[] { '=' }, 2);
                    if (split.Length < 2) continue;
                    message = string.Concat(split.Skip(1));
                    Console.WriteLine("message changed to {0}", message);
                }
                if (input.StartsWith("delay", StringComparison.OrdinalIgnoreCase)) {
                    var split = input.Split(new[] { '=' },2);
                    if (split.Length < 2) continue;
                    if (int.TryParse(split[1], out delay)) {
                        Console.WriteLine("delay changed to {0}", delay);
                    }
                }
                if (input.Equals("diagnostics", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("message = {0}\ndelay = {1}\ntime till next = {2} seconds", message, delay, delay - (DateTime.Now - lastSend).Seconds);
                }
            }
        }
    }
}