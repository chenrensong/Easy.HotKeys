using Easy.HotKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Demo
{
    /// <summary>
    /// 当前.net standard 2.0 暂时不支持 HwndSource，通过IHwndHook抽象来实现HwndSource注入，稍微麻烦一些
    /// </summary>
    public class RealHwndHook : IHwndHook
    {
        private readonly HwndSource _handleSource = new HwndSource(new HwndSourceParameters());
        private List<HwndHook> _weakReferences = new List<HwndHook>();
        public IntPtr Handle
        {
            get
            {
                return _handleSource.Handle;
            }
        }

        public void AddHook(Easy.HotKeys.HwndHook hook)
        {
            _weakReferences.Add(hook);
            _handleSource.AddHook(OnMessagesHandler);
        }

        public void Dispose()
        {
            _handleSource.Dispose();
            _weakReferences.Clear();
        }

        public void RemoveHook(Easy.HotKeys.HwndHook hook)
        {
            if (hook == null)
            {
                return;
            }
            HwndHook target = null;
            foreach (var item in _weakReferences)
            {
                if (item.Equals(hook))
                {
                    target = item;
                    return;
                }
            }
            if (target != null)
            {
                _weakReferences.Remove(target);
            }
        }

        private IntPtr OnMessagesHandler(IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (var hwnd in _weakReferences)
            {
                var result = hwnd(handle, message, wParam, lParam, ref handled);
                if (handled)
                {
                    return result;
                }
            }
            return IntPtr.Zero;
        }
    }
}
