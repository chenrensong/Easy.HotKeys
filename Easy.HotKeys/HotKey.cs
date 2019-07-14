using Easy.WinAPI.Input;
using System.Text;
using System.Windows.Input;

namespace Easy.HotKeys
{
    public class HotKey
    {
        public HotKey(EasyKey key, EasyModifierKeys modifierKeys)
        {
            Key = key;
            ModifierKeys = modifierKeys;
        }

        public HotKey()
        {
        }

        public EasyKey Key { get; set; }

        public EasyModifierKeys ModifierKeys { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() == typeof(HotKey))
            {
                var o = (HotKey)obj;
                return Equals(o.Key, Key) && Equals(o.ModifierKeys, ModifierKeys);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Key.GetHashCode() * 397) ^ ModifierKeys.GetHashCode();
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var keys = ModifierKeys.ToString().Split(',');
            foreach (var key in keys) 
            {
                builder.Append(key);
                builder.Append("+");
            }
            builder.Append($"{Key}");
            return builder.ToString();
        }


    }
}
