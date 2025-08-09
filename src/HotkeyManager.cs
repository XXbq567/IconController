using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace IconController
{
    public sealed class HotkeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly int _id;
        private readonly Action _action;
        private readonly HwndSource _src;

        [DllImport("user32")] static extern bool RegisterHotKey(IntPtr h, int id, uint f, uint v);
        [DllImport("user32")] static extern bool UnregisterHotKey(IntPtr h, int id);

        public HotkeyManager(IntPtr h, string s, Action a)
        {
            _action = a;
            _id = GetHashCode();
            _src = HwndSource.FromHwnd(h);
            _src.AddHook(WndProc);
            var (f, k) = Parse(s);
            RegisterHotKey(h, _id, f, (uint)KeyInterop.VirtualKeyFromKey(k));
        }

        private IntPtr WndProc(IntPtr h, int m, IntPtr w, IntPtr l, ref bool handled)
        {
            if (m == WM_HOTKEY && w.ToInt32() == _id) _action();
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey(_src.Handle, _id);
            _src.RemoveHook(WndProc);
        }

        private static (uint, Key) Parse(string s)
        {
            uint f = 0;
            foreach (var p in s.Split('+'))
            {
                var t = p.Trim();
                if (t.Equals("Ctrl",  StringComparison.OrdinalIgnoreCase)) f |= 0x2;
                else if (t.Equals("Alt", StringComparison.OrdinalIgnoreCase)) f |= 0x1;
                else if (t.Equals("Shift", StringComparison.OrdinalIgnoreCase)) f |= 0x4;
                else return (f, (Key)Enum.Parse(typeof(Key), t, true));
            }
            return (f, Key.None);
        }
    }
}
