using System;
using System.Windows;
using System.Windows.Threading;

namespace DesktopToggle
{
    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var settings = Settings.Load();

            // 首次运行强制显示设置窗口
            if (settings.FirstRun)
            {
                settings.FirstRun = false;
                settings.Save();
                new MainWindow().ShowDialog();
            }
            else
            {
                // 以后启动只留托盘
                _ = new MainWindow();  // 构造里自动隐藏
            }

            app.Run();
        }
    }
}
