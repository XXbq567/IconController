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

            SaveBtn.Click += (_, __) => Save();
            CancelBtn.Click += (_, __) => { if (!_s.FirstRun) Hide(); else Close(); };
            ChangeBtn.Click += (_, __) => OpenCaptureWindow();
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

            w.KeyDown += (s, e) =>
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
