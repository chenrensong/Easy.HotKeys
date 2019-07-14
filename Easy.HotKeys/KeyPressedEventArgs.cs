using System;

namespace Easy.HotKeys
{
    public class KeyPressedEventArgs : EventArgs
    {
        public KeyPressedEventArgs(HotKey hotKey)
        {
            HotKey = hotKey;
        }

        public HotKey HotKey { get; private set; }
    }
}