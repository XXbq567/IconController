using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// 不再 using System.Windows.Forms; 避免与 WPF 冲突
// NotifyIcon 用完全限定名

public partial class MainWindow : Window
{
    private readonly Settings _settings = Settings.Load();
    private HotkeyManager? _hk;

    // 注意：写成完全限定名
    private readonly System.Windows.Forms.NotifyIcon _tray = new()
    {
        Text = "Desktop Icon Toggle",
        Icon = System.Drawing.SystemIcons.Application,
        Visible = true
    };

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _settings;

        Loaded += (_, __) => { Hide(); }; // 启动后隐藏主窗口

        _tray.DoubleClick += (_, __) => Show();
        _tray.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
        _tray.ContextMenuStrip.Items.Add("设置", null, (_, __) => Show());
        _tray.ContextMenuStrip.Items.Add("退出", null, (_, __) => Close());

        ApplyAutoStart();
        RestartHotkey();
    }

    private void Save()
    {
        _settings.Enabled   = EnabledBox.IsChecked == true;
        _settings.Hotkey    = HotkeyBox.Text;
        _settings.AutoStart = AutoStartBox.IsChecked == true;
        _settings.ShowTrayIcon = ShowTrayIconBox.IsChecked == true;
        _settings.Save();

        ApplyAutoStart();
        RestartHotkey();
        _tray.Visible = _settings.ShowTrayIcon;
        Hide();
    }

    private void ApplyAutoStart()
    {
        var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        var exePath = Process.GetCurrentProcess().MainModule!.FileName;
        if (_settings.AutoStart)
            rk?.SetValue("DesktopToggle", exePath);
        else
            rk?.DeleteValue("DesktopToggle", false);
    }

    private void RestartHotkey()
    {
        _hk?.Dispose();
        if (_settings.Enabled)
        {
            _hk = new HotkeyManager(
                new WindowInteropHelper(this).Handle,
                _settings.Hotkey,
                ToggleIcons);
        }
    }

    private static void ToggleIcons()
    {
        const string key = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        using var rk = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key);
        var hide = (int?)rk.GetValue("HideIcons") == 1 ? 0 : 1;
        rk.SetValue("HideIcons", hide, Microsoft.Win32.RegistryValueKind.DWord);
        RefreshDesktop();
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

    private static void RefreshDesktop()
    {
        SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
    }

    protected override void OnClosed(EventArgs e)
    {
        _hk?.Dispose();
        _tray.Dispose();
        base.OnClosed(e);
    }
}
