using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Timer = System.Timers.Timer;

namespace DesktopToggle
{
    public partial class MainWindow : Window
    {
        private readonly Settings _settings = Settings.Load();
        private HotkeyManager _hk;
        private readonly NotifyIcon _tray;
        private readonly Timer _regPollTimer;

        public MainWindow()
        {
            InitializeComponent();

            // 托盘图标
            _tray = new NotifyIcon
            {
                Text = "Desktop Icon Toggle",
                Icon = System.Drawing.SystemIcons.Application,
                Visible = _settings.ShowTrayIcon
            };
            _tray.DoubleClick += (_, __) => Show();
            _tray.ContextMenuStrip = new ContextMenuStrip();
            _tray.ContextMenuStrip.Items.Add("设置", null, (_, __) => Show());
            _tray.ContextMenuStrip.Items.Add("退出", null, (_, __) => Close());

            // 界面绑定
            EnabledBox.IsChecked   = _settings.Enabled;
            HotkeyBox.Text         = _settings.Hotkey;
            AutoStartBox.IsChecked = _settings.AutoStart;
            ShowTrayIconBox.IsChecked = _settings.ShowTrayIcon;

            SaveBtn.Click    += (_, __) => Save();
            CancelBtn.Click  += (_, __) => { if (!_settings.FirstRun) Hide(); else Close(); };

            // 首次同步隐藏/显示状态
            SetIconsVisible(!_settings.HideIcons);

            // 每 2 秒轮询注册表，若用户用右键改回，同步内部状态
            _regPollTimer = new Timer(2000) { AutoReset = true };
            _regPollTimer.Elapsed += (_, __) =>
            {
                bool currentHidden = IsIconsHidden();
                if (currentHidden != _settings.HideIcons)
                {
                    _settings.HideIcons = currentHidden;
                    _settings.Save();
                }
            };
            _regPollTimer.Start();

            // 启动后隐藏主窗口
            Loaded += (_, __) =>
            {
                if (!_settings.FirstRun) Hide();
            };

            ApplyAutoStart();
            RestartHotkey();
        }

        private void Save()
        {
            _settings.Enabled   = EnabledBox.IsChecked == true;
            _settings.Hotkey    = HotkeyBox.Text;
            _settings.AutoStart = AutoStartBox.IsChecked == true;
            _settings.ShowTrayIcon = ShowTrayIconBox.IsChecked == true;

            _settings.HideIcons  = IsIconsHidden(); // 保存当前状态
            _settings.Save();

            ApplyAutoStart();
            RestartHotkey();
            _tray.Visible = _settings.ShowTrayIcon;
            Hide();
        }

        private bool IsIconsHidden() =>
            Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "HideIcons", 0) is int val && val == 1;

        private void SetIconsVisible(bool visible)
        {
            int hide = visible ? 0 : 1;
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "HideIcons", hide, Microsoft.Win32.RegistryValueKind.DWord);
            RefreshDesktop();
        }

        private void ToggleIcons()
        {
            bool current = IsIconsHidden();
            SetIconsVisible(current);
            _settings.HideIcons = !current;
            _settings.Save();
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

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private static void RefreshDesktop()
        {
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void OnClosed(EventArgs e)
        {
            _regPollTimer.Stop();
            _regPollTimer.Dispose();
            _hk?.Dispose();
            _tray.Dispose();
            base.OnClosed(e);
        }
    }
}
