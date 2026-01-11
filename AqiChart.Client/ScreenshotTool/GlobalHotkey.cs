using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace AqiChart.Client.ScreenshotTool
{
    public class GlobalHotkey : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly int _hotkeyId;
        private bool _disposed = false;
        private HwndSource _source;

        public event EventHandler HotkeyPressed;

        public GlobalHotkey(Window window, int hotkeyId, ModifierKeys modifiers, Key key)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            _windowHandle = new WindowInteropHelper(window).EnsureHandle();
            _hotkeyId = hotkeyId;

            // 转换WPF修饰键到Windows修饰键
            uint fsModifiers = ConvertModifierKeys(modifiers);
            fsModifiers |= NativeMethods.MOD_NOREPEAT; // 防止重复触发

            // 获取Windows虚拟键码
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            // 注册热键
            bool success = NativeMethods.RegisterHotKey(_windowHandle, _hotkeyId, fsModifiers, vk);
            if (!success)
            {
                int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"注册全局热键失败 (错误代码: {errorCode})");
            }

            // 添加消息钩子
            _source = HwndSource.FromHwnd(_windowHandle);
            if (_source != null)
            {
                _source.AddHook(WndProc);
            }
        }

        private uint ConvertModifierKeys(ModifierKeys modifiers)
        {
            uint result = 0;

            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                result |= NativeMethods.MOD_ALT;

            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                result |= NativeMethods.MOD_CONTROL;

            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                result |= NativeMethods.MOD_SHIFT;

            if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                result |= NativeMethods.MOD_WIN;

            return result;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 清理托管资源
                    if (_source != null)
                    {
                        _source.RemoveHook(WndProc);
                        _source = null;
                    }
                }

                // 注销热键
                NativeMethods.UnregisterHotKey(_windowHandle, _hotkeyId);

                _disposed = true;
            }
        }

        ~GlobalHotkey()
        {
            Dispose(false);
        }
    }
}
