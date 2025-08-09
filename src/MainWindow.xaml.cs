using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopToggle
{
    public partial class MainWindow : Window
    {
        private readonly Settings _settings = Settings.Load();
        private HotkeyManager? _hk;
        private readonly System.Windows.Forms.NotifyIcon _tray;

        public MainWindow()
        {
            InitializeComponent();   // 现在能找到，因为 x:Name 已匹配

            _tray = new System.Windows.Forms.NotifyIcon
            {
                Text = "Desktop Icon Toggle",
                Icon = System.Drawing.SystemIcons.Application,
                Visible = _settings.ShowTrayIcon
            };

            // 绑定控件
            EnabledBox.IsChecked = _settings.Enabled;
            HotkeyBox.Text = _settings.Hotkey;
            AutoStartBox.IsChecked = _settings.AutoStart;
            ShowTrayIconBox.IsChecked = _settings.ShowTrayIcon;

            SaveBtn.Click += (_, __) => Save();
            CancelBtn.Click += (_, __) => Close();
            _tray.DoubleClick += (_, __) => Show();

            _tray.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _tray.ContextMenuStrip.Items.Add("设置", null, (_, __) => Show());
            _tray.ContextMenuStrip.Items.Add("退出", null, (_, __) => Close());

            Loaded += (_, __) => Hide(); // 启动即隐藏

            ApplyAutoStart();
            RestartHotkey();
        }

        private void Save()
        {
            _settings.Enabled = EnabledBox.IsChecked == true;
            _settings.Hotkey = HotkeyBox.Text;
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
                _hk = new HotkeyManager(
                    new WindowInteropHelper(this).Handle,
                    _settings.Hotkey,
                    ToggleIcons);
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
}
