using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.HotKeys
{
    public interface IHwndHook : IDisposable
    {
        IntPtr Handle { get; }

        void AddHook(HwndHook hook);
        void RemoveHook(HwndHook hook);
    }

    public delegate IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);
}
