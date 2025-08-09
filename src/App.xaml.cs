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

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!createdNew)
            {
                BringOtherToFront();
                Shutdown();
                return;
            }

            var s = Settings.Load();
            if (s.FirstRun)
            {
                s.FirstRun = false;
                s.Save();
                base.OnStartup(e);          // 正常启动 MainWindow
            }
            else
            {
                base.OnStartup(e);
                if (MainWindow is MainWindow mw) mw.Hide(); // 后台运行
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
