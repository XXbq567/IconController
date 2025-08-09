using System;
using System.Windows;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        app.Run(new App());
    }
}

public class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new MainWindow().Hide(); // 后台运行
    }
}
