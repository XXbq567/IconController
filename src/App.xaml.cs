using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace IconController
{
    public partial class App : Application
    {
        private static readonly Mutex Mutex = new(true, "IconController_SingleInstance", out bool createdNew);

        [STAThread]
        public static void Main()
        {
            if (!createdNew)
            {
                BringOtherToFront();
                return;
            }
            var app = new App();
            var s = Settings.Load();
            if (s.FirstRun)
            {
                s.FirstRun = false;
                s.Save();
                app.Run(new MainWindow());
            }
            else
            {
                _ = new MainWindow(); // 构造里隐藏
                app.Run();
            }
        }

        private static void BringOtherToFront()
        {
            var proc = Process.GetCurrentProcess();
            foreach (var p in Process.GetProcessesByName(proc.ProcessName))
            {
                if (p.Id == proc.Id) continue;
                SetForegroundWindow(p.MainWindowHandle);
                break;
            }
        }

        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
