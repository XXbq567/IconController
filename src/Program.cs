using System;
using System.Windows;
using System.Windows.Threading;

namespace DesktopToggle
{
    internal static class Program
    {
        [STAThread]                         // 需要 using System;
        public static void Main()
        {
            var app = new Application();
            // 主窗口自己创建，立即隐藏
            var mw = new MainWindow();
            app.Run();
        }
    }
}
