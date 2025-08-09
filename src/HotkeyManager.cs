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
