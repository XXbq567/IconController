using System;
using System.Threading;
using System.Windows;

namespace IconController
{
    public partial class App : Application
    {
        private static Mutex _mutex = new(true, "IconController_SingleInstance", out bool createdNew);

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!createdNew)
            {
                Native.BringToFront();
                Shutdown();
                return;
            }
            var s = Settings.Load();
            if (s.FirstRun)
            {
                s.FirstRun = false;
                s.Save();
                new MainWindow().ShowDialog();
            }
            else
            {
                _ = new MainWindow(); // 构造里隐藏
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }

    internal static class Native
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void BringToFront()
        {
            var proc = Process.GetCurrentProcess();
            foreach (var p in Process.GetProcessesByName(proc.ProcessName))
                if (p.Id != proc.Id) { SetForegroundWindow(p.MainWindowHandle); break; }
        }
    }
}
