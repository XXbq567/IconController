using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
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

        private void ChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            var w = new Window
            {
                Title = "请按下新快捷键…",
                Width = 300, Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = new System.Windows.Controls.TextBlock
                {
                    Text = "请按下组合键…",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                },
                Background = (System.Windows.Media.Brush)FindResource("BgBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("FgBrush")
            };

            w.KeyDown += (_, k) =>
            {
                var mod = "";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) mod += "Ctrl+";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) mod += "Alt+";
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) mod += "Shift+";
                var key = k.Key == Key.System ? k.SystemKey : k.Key;
                if (key != Key.None && !key.ToString().StartsWith("Left") && !key.ToString().StartsWith("Right"))
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
                Content = new System.Windows.Controls.StackPanel
                {
                    Children =
                    {
                        new System.Windows.Controls.TextBlock
                        {
                            Text = $"设为 {newHotkey} 吗？",
                            Margin = new Thickness(10)
                        },
                        new System.Windows.Controls.StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Children =
                            {
                                new System.Windows.Controls.Button
                                {
                                    Content = "保存设置", Width = 90, Margin = new Thickness(5),
                                    Background = (System.Windows.Media.Brush)FindResource("BgBrush")
                                }.Also(b => b.Click += (_, __) => { _s.Hotkey = newHotkey; dlg.Close(); }),
                                new System.Windows.Controls.Button
                                {
                                    Content = "重新设置", Width = 90, Margin = new Thickness(5),
                                    Background = (System.Windows.Media.Brush)FindResource("BgBrush")
                                }.Also(b => b.Click += (_, __) => { dlg.Close(); ChangeBtn_Click(null, null); })
                            }
                        }
                    }
                },
                Background = (System.Windows.Media.Brush)FindResource("BgBrush"),
                Foreground = (System.Windows.Media.Brush)FindResource("FgBrush")
            };
            dlg.ShowDialog();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
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

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_s.FirstRun) Hide(); else Close();
        }

        private void ApplyAutoStart()
        {
            var rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var exe = Process.GetCurrentProcess().MainModule!.FileName;
            if (_s.AutoStart)
                rk?.SetValue("IconController", exe);
            else
                rk?.DeleteValue("IconController", false
