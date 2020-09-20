using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using System.Threading.Tasks;

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

            int calcDelay = delay;

            var stopwatch = new Stopwatch();

            bool paused = false;

            Console.WriteLine("Launching program with message '{0}' and {1} sec delay", message, delay);

            //Set spamming work to a timer, with initial delay of 1s (to fire the spamming event immediately)
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (t, a) => {
                //Change the delay to the real delay
                timer.Interval = delay * 1000;
                //Restart the stopwatch for diagnostic purposes
                stopwatch.Restart();
                Process[] processes = Process.GetProcessesByName("discord");
                //Check if discord is open
                if (processes.Length == 0) {
                    Console.WriteLine("\nDiscord is not open. Please close the window or type \"retry\" to try again.");
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

                //Feed keystrokes into discord
                SendKeys.SendWait(message + "{ENTER}");
                messages++;
                
                calcDelay = delay;
            };
            timer.Start();

            //Handle user input
            while (true) {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                    timer.Dispose();
                    Environment.Exit(Environment.ExitCode);
                }
                if (input.Equals("pause", StringComparison.OrdinalIgnoreCase)) {
                    if (paused) {
                        Console.WriteLine("program already paused");
                        continue;
                    }
                    stopwatch.Stop();
                    timer.Stop();
                    paused = true;
                    Console.WriteLine("paused");
                }
                if (input.Equals("unpause", StringComparison.OrdinalIgnoreCase)) {
                    if(!paused) {
                        Console.WriteLine("program currently isn't paused");
                        continue;
                    }
                    stopwatch.Start();
                    timer.Start();
                    //Set timer delay to remaining time after the pause
                    timer.Interval = delay * 1000 + stopwatch.Elapsed.Milliseconds;
                    paused = false;
                    Console.WriteLine("unpaused");
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
                    Console.WriteLine("message = {0}\ndelay = {1}\ntime till next = {2} seconds\nmessages sent = {3}", message, delay, calcDelay - stopwatch.Elapsed.Seconds, messages);
                    if (paused) Console.WriteLine("currently paused");
                }
            }
        }
    }
}