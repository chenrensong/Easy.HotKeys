using Easy.WinAPI;
using Easy.WinAPI.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Easy.HotKeys
{
    public class EasyHotKey : IDisposable
    {

        private readonly Dictionary<HotKey, int> _registered = new Dictionary<HotKey, int>();

        //private readonly HwndSource _handleSource = new HwndSource(new HwndSourceParameters());

        private bool _disposed = false;

        private readonly IHwndHook _hwndHook = null;

        public event EventHandler<KeyPressedEventArgs> KeyPressed;


        public EasyHotKey(IHwndHook hwndHook)
        {
            _hwndHook = hwndHook;
            _hwndHook.AddHook(OnMessagesHandler);
        }

        ~EasyHotKey()
        {
            this.Dispose(false);
        }

        public HotKey Register(EasyKey key, EasyModifierKeys modifiers)
        {
            var hotKey = new HotKey(key, modifiers);
            Register(hotKey);
            return hotKey;
        }

        public void Register(HotKey hotKey)
        {
            if (_registered.ContainsKey(hotKey))
            {
                throw new ArgumentException("The specified hot key is already registered.");
            }
            var id = GetNewKeyId();
            if (!EasyWinAPI.RegisterHotKey(_hwndHook.Handle, id, hotKey.ModifierKeys, hotKey.Key))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Can't register the hot key.");
            }
            _registered.Add(hotKey, id);
        }

        public void Register(uint key, uint modifierKeys)
        {
            var key1 = EasyKeyInterop.KeyFromVirtualKey((int)key);
            var modifierKeys1 = (EasyModifierKeys)modifierKeys;
            var hotKey = new HotKey(key1, modifierKeys1);
            if (_registered.ContainsKey(hotKey))
            {
                throw new ArgumentException("The specified hot key is already registered.");
            }
            var id = GetNewKeyId();
            if (!EasyWinAPI.RegisterHotKey(_hwndHook.Handle, id, modifierKeys, key))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Can't register the hot key.");
            }
            _registered.Add(hotKey, id);
        }

        public void Unregister(EasyKey key, EasyModifierKeys modifiers)
        {
            var hotKey = new HotKey(key, modifiers);
            Unregister(hotKey);
        }

        public void Unregister(HotKey hotKey)
        {
            int id;
            if (_registered.TryGetValue(hotKey, out id))
            {
                EasyWinAPI.UnRegisterHotKey(_hwndHook.Handle, id);
                _registered.Remove(hotKey);
            }
        }

        public void UnregisterAll()
        {
            foreach (var hotKey in _registered)
            {
                EasyWinAPI.UnRegisterHotKey(_hwndHook.Handle, hotKey.Value);
            }
            _registered.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            UnregisterAll();
            _hwndHook.RemoveHook(OnMessagesHandler);
            _hwndHook.Dispose();
            if (disposing)
            {
            }
            _disposed = true;
        }


        private int GetNewKeyId()
        {
            return _registered.Any() ? _registered.Values.Max() + 1 : 0;
        }

        private IntPtr OnMessagesHandler(IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message == 0x0312)
            {
                var key = EasyKeyInterop.KeyFromVirtualKey(((int)lParam >> 16) & 0xFFFF);
                var modifierKeys = (EasyModifierKeys)((int)lParam & 0xFFFF);
                OnKeyPressed(key, modifierKeys);
                handled = true;
                return new IntPtr(1);
            }
            return IntPtr.Zero;
        }

        private void OnKeyPressed(EasyKey key, EasyModifierKeys modifierKeys)
        {
            if (KeyPressed != null)
            {
                var hotKey = new HotKey(key, modifierKeys);
                var args = new KeyPressedEventArgs(hotKey);
                KeyPressed(this, args);
            }
        }
    }
}
