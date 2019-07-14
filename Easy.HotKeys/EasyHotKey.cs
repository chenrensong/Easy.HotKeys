using Easy.WinAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Easy.HotKeys
{
    public class EasyHotKey : IDisposable
    {

        private readonly Dictionary<HotKey, int> _registered = new Dictionary<HotKey, int>();

        private readonly HwndSource _handleSource = new HwndSource(new HwndSourceParameters());

        private bool _disposed = false;

        public event EventHandler<KeyPressedEventArgs> KeyPressed;


        public EasyHotKey()
        {
            _handleSource.AddHook(OnMessagesHandler);
        }

        ~EasyHotKey()
        {
            this.Dispose(false);
        }

        public HotKey Register(Key key, ModifierKeys modifiers)
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
            var key = KeyInterop.VirtualKeyFromKey(hotKey.Key);
            if (!EasyWinAPI.RegisterHotKey(_handleSource.Handle, id, (uint)hotKey.ModifierKeys, (uint)key))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Can't register the hot key.");
            }
            _registered.Add(hotKey, id);
        }

        public void Unregister(Key key, ModifierKeys modifiers)
        {
            var hotKey = new HotKey(key, modifiers);
            Unregister(hotKey);
        }

        public void Unregister(HotKey hotKey)
        {
            int id;
            if (_registered.TryGetValue(hotKey, out id))
            {
                EasyWinAPI.UnRegisterHotKey(_handleSource.Handle, id);
                _registered.Remove(hotKey);
            }
        }

        public void UnregisterAll()
        {
            foreach (var hotKey in _registered)
            {
                EasyWinAPI.UnRegisterHotKey(_handleSource.Handle, hotKey.Value);
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
            _handleSource.RemoveHook(OnMessagesHandler);
            _handleSource.Dispose();
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
                var key = KeyInterop.KeyFromVirtualKey(((int)lParam >> 16) & 0xFFFF);
                var modifierKeys = (ModifierKeys)((int)lParam & 0xFFFF);
                OnKeyPressed(key, modifierKeys);
                handled = true;
                return new IntPtr(1);
            }
            return IntPtr.Zero;
        }

        private void OnKeyPressed(Key key, ModifierKeys modifierKeys)
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
