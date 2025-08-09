using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace DesktopToggle
{
    public class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly int _id;
        private readonly Action _callback;
        private readonly HwndSource _source;

        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyManager(IntPtr hwnd, string shortcut, Action callback)
        {
            _callback = callback;
            _id = GetHashCode();
            _source = HwndSource.FromHwnd(hwnd);
            _source.AddHook(WndProc);

            var (mod, key) = Parse(shortcut);
            RegisterHotKey(hwnd, _id, mod, (uint)KeyInterop.VirtualKeyFromKey(key));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
                _callback();
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey(_source.Handle, _id);
            _source.RemoveHook(WndProc);
        }

        private static (uint mod, Key key) Parse(string s)
        {
            uint mod = 0;
            var parts = s.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                if (p.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)) mod |= 0x0002;
                else if (p.Equals("Alt", StringComparison.OrdinalIgnoreCase)) mod |= 0x0001;
                else if (p.Equals("Shift", StringComparison.OrdinalIgnoreCase)) mod |= 0x0004;
                else return (mod, (Key)Enum.Parse(typeof(Key), p, true));
            }
            return (mod, Key.None);
        }
    }
}
