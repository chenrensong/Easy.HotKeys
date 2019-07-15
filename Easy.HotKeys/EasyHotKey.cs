using Easy.WinAPI;
using Easy.WinAPI.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Easy.HotKeys
{
    public class EasyHotKey : IDisposable
    {

        private readonly static Dictionary<EasyKey, EasyModifierKeys> _keyMap = new Dictionary<EasyKey, EasyModifierKeys>()
        {
                { EasyKey.LeftAlt,EasyModifierKeys.Alt},
                { EasyKey.RightAlt,EasyModifierKeys.Alt},
                { EasyKey.LWin,EasyModifierKeys.Windows},
                { EasyKey.RWin,EasyModifierKeys.Windows},
                { EasyKey.LeftCtrl,EasyModifierKeys.Control},
                { EasyKey.RightCtrl,EasyModifierKeys.Control},
                { EasyKey.LeftShift,EasyModifierKeys.Shift},
                { EasyKey.RightShift,EasyModifierKeys.Shift}
        };

        private List<EasyKey> _pressKeys = new List<EasyKey>();

        private readonly Dictionary<HotKey, int> _registered = new Dictionary<HotKey, int>();

        private bool _disposed = false;

        private IntPtr _hookId;

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        /// <summary>
        /// 避免被垃圾回收
        /// </summary>
        private static LowLevelKeyboardProc _hookproc;

        public EasyHotKey()
        {
            _hookproc = new LowLevelKeyboardProc(HookCallBack);
            _hookId = EasyWinAPI.SetKeyboardHookEx(_hookproc);
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
            _registered.Add(hotKey, id);
        }

        public void Register(uint key, uint modifierKeys)
        {
            var key1 = EasyKeyInterop.KeyFromVirtualKey((int)key);
            var modifierKeys1 = (EasyModifierKeys)modifierKeys;
            var hotKey = new HotKey(key1, modifierKeys1);
            Register(hotKey);
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
                _registered.Remove(hotKey);
            }
        }

        public void UnregisterAll()
        {
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
            if (disposing)
            {
            }
            UnregisterAll();
            EasyWinAPI.UnhookWindowsHookEx(this._hookId);
            _disposed = true;
        }

        /// <summary>
        /// 是否是Ctrl、Alt、Shift
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool TryGetModifierKey(EasyKey key, out EasyModifierKeys modifierKeys)
        {
            modifierKeys = EasyModifierKeys.None;
            bool hasFlag = false;
            if (hasFlag = _keyMap.ContainsKey(key))
            {
                modifierKeys = _keyMap[key];
            }
            return hasFlag;
        }

        private int GetNewKeyId()
        {
            return _registered.Any() ? _registered.Values.Max() + 1 : 0;
        }

        private IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int keyState = (int)wParam;
            int vkCode = Marshal.ReadInt32(lParam);
            var keyPressed = EasyKeyInterop.KeyFromVirtualKey(vkCode);
            if (nCode >= 0 && (wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
            {
                if (_pressKeys.IndexOf(keyPressed) < 0)
                {
                    _pressKeys.Add(keyPressed);
                }
                EasyModifierKeys modifierKeys = default(EasyModifierKeys);
                EasyKey easyKey = default(EasyKey);
                foreach (var key in _pressKeys)
                {
                    if (TryGetModifierKey(key, out EasyModifierKeys keys))
                    {
                        modifierKeys = modifierKeys | keys;
                    }
                    else
                    {
                        easyKey = easyKey | key;
                    }
                }
                var hotKey = new HotKey(easyKey, modifierKeys);
                if (_registered.ContainsKey(hotKey))
                {
                    OnKeyPressed(hotKey);
                }
            }
            if (nCode >= 0 && (wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP))
            {
                _pressKeys.Remove(keyPressed);
            }
            return EasyWinAPI.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }


        private void OnKeyPressed(HotKey hotKey)
        {
            if (KeyPressed != null)
            {
                var args = new KeyPressedEventArgs(hotKey);
                KeyPressed(this, args);
            }
        }
    }
}

