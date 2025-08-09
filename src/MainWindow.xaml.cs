using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Timer = System.Timers.Timer;

namespace IconController
{
    public partial class MainWindow : Window
    {
        private readonly Settings _s = Settings.Load();
        private HotkeyManager _hk;
        private readonly NotifyIcon _tray;
        private Timer _poll;

        public MainWindow()
        {
            InitializeComponent();
            _tray = new NotifyIcon
            {
                Text = "IconController",
                Icon = System.Drawing.SystemIcons.Application,
                Visible = _s.ShowTrayIcon
            };
            _tray.DoubleClick += (_, __) => Show();
            _tray.ContextMenuStrip = new ContextMenuStrip();
            _tray.ContextMenuStrip.Items.Add("设置", null, (_, __) => Show());
            _tray.ContextMenuStrip.Items.Add("退出", null, (_, __) => Close());

            Loaded += (_, __) => { if (!_s.FirstRun) Hide(); };

            BindUi();
            ApplyAutoStart();
            RestartHotkey();
            StartPolling();
        }

        private void BindUi()
        {
            EnabledBox.IsChecked = _s.Enabled;
            HotkeyBox.Text = _s.Hotkey;
            AutoStartBox.IsChecked = _s.AutoStart;
            ShowTrayBox.IsChecked = _s.ShowTrayIcon;
        }

        private void ChangeBtn_Click(object sender, RoutedEventArgs e) => OpenCaptureWindow();
        private void SaveBtn_Click(object sender, RoutedEventArgs e) => Save();
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_s.FirstRun) Hide(); else Close();
        }

        private void OpenCaptureWindow()
        {
            var w = new Window
            {
                Title = "请按下新快捷键",
                Width = 300, Height = 120,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (System.Windows.Media.Brush)FindResource("BgBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("FgBrush"),
                Content = new TextBlock
                {
                    Text = "请按下组合键…",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            w.KeyDown += (_, e) =>
            {
                var mod = "";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) mod += "Ctrl+";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) mod += "Alt+";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) mod += "Shift+";
                var key = e.Key == Key.System ? e.SystemKey : e.Key;
                if (key != Key.None && key != Key.LeftCtrl && key != Key.RightCtrl &&
                    key != Key.LeftAlt && key != Key.RightAlt &&
                    key != Key.LeftShift && key != Key.RightShift)
                {
                    var newHotkey = mod + key;
                    w.Close();
                    AskConfirm(newHotkey);
                }
            };
            w.ShowDialog();
        }

        private void AskConfirm(string newHotkey)
        {
            var dlg = new Window
            {
                Title = "确认快捷键",
                Width = 280, Height = 120,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (System.Windows.Media.Brush)FindResource("BgBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("FgBrush")
            };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = $"确认把快捷键设为 {newHotkey} 吗？",
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            btnPanel.Children.Add(new Button
            {
                Content = "保存设置", Width = 90, Margin = new Thickness(5)
            }.Also(b => b.Click += (_, __) => { _s.Hotkey = newHotkey; dlg.Close(); }));
            btnPanel.Children.Add(new Button
            {
                Content = "重新设置", Width = 90, Margin = new Thickness(5)
            }.Also(b => b.Click += (_, __) => { dlg.Close(); OpenCaptureWindow(); }));
            panel.Children.Add(btnPanel);
            dlg.Content = panel;
            dlg.ShowDialog();
        }

        private void Save()
        {
            _s.Enabled = EnabledBox.IsChecked == true;
            _s.AutoStart = AutoStartBox.IsChecked == true;
            _s.ShowTrayIcon = ShowTrayBox.IsChecked == true;
            _s.Save();
            ApplyAutoStart();
            RestartHotkey();
            _tray.Visible = _s.ShowTrayIcon;
            Hide();
        }

        private void ApplyAutoStart()
        {
            var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            var exe = Process.GetCurrentProcess().MainModule!.FileName;
            if (_s.AutoStart) rk?.SetValue("IconController", exe);
            else rk?.DeleteValue("IconController", false);
        }

        private void RestartHotkey()
        {
            _hk?.Dispose();
            if (_s.Enabled)
                _hk = new HotkeyManager(
                    new WindowInteropHelper(this).Handle,
                    _s.Hotkey,
                    () =>
                    {
                        bool cur = IsIconsHidden();
                        SetIconsVisible(cur);
                        _s.HideIcons = !cur;
                        _s.Save();
                    });
        }

        private bool IsIconsHidden() =>
            (int?)Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "HideIcons", 0) == 1;

        private void SetIconsVisible(bool show)
        {
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                "HideIcons", show ? 0 : 1, Microsoft.Win32.RegistryValueKind.DWord);
            RefreshDesktop();
        }

        private void StartPolling()
        {
            _poll = new Timer(1500);
            _poll.Elapsed += (_, __) =>
            {
                bool cur = IsIconsHidden();
                if (cur != _s.HideIcons) { _s.HideIcons = cur; _s.Save(); }
            };
            _poll.Start();
        }

        [DllImport("shell32.dll")] private static extern void SHChangeNotify(int w, int u, IntPtr d1, IntPtr d2);
        private static void RefreshDesktop() => SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
    }

    internal static class Ext
    {
        public static T Also<T>(this T obj, Action<T> act) { act(obj); return obj; }
    }
}
