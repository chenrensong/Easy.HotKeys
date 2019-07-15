# Easy.HotKeys
Windows Global HotKeys

## 版本日历

### 1.2版本升级实现，采用Hook键盘的方式，代码精简，不需要外部注入，只依赖`Easy.WinAPI`不需要其他任何依赖项，当然由于采用了WinAPI,该项目只能用于Windows平台。

### 1.1版本改造为.Net Standard 2.0  暂时不支持 HwndSource，使用Easy.HotKeys时候，需要通过IHwndHook抽象来实现HwndSource注入。

### 1.0版本采用.Net Framework实现，主要依赖HwndSource来Hook API

