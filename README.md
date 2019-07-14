# Easy.HotKeys
Windows Global HotKeys

由于.net standard 2.0 暂时不支持`HwndSource`，使用Easy.HotKeys时候，需要通过`IHwndHook`接口来实现`HwndSource`注入，稍微麻烦一些，具体代码如下：

```csharp
  
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
```

需要在使用的时候新建`RealHwndHook`类,初始化`EasyHotKey`的时候传入

```csharp
     private readonly EasyHotKey _easyHotKey = new EasyHotKey(new RealHwndHook());
```
