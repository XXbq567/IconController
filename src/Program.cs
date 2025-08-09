using System.Windows;

namespace DesktopToggle
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            app.Run();   // 不传入 Window，由 MainWindow 自己在构造里 Hide
            // MainWindow 实例会在托盘里保持后台
        }
    }
}
