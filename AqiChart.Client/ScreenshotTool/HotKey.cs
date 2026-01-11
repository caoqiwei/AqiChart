using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WeChat.Client.ScreenshotTool
{
    /// <summary>
    /// 全局热键管理
    /// </summary>
    public class HotKey : IDisposable
    {
        private readonly Key _key;
        private readonly ModifierKeys _modifiers;
        private readonly Action<HotKey> _action;
        private readonly int _id;
        private static int _nextId = 100;
        private bool _isRegistered;

        // Windows API 常量
        private const int WM_HOTKEY = 0x0312;

        public HotKey(Key key, ModifierKeys modifiers, Action<HotKey> action)
        {
            _key = key;
            _modifiers = modifiers;
            _action = action;
            _id = _nextId++;
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        public bool Register()
        {
            try
            {
                // 检查是否已注册
                if (_isRegistered)
                {
                    Debug.WriteLine("热键已注册，跳过重复注册");
                    return true;
                }

                // 获取主窗口句柄
                var mainWindow = System.Windows.Application.Current?.MainWindow;
                if (mainWindow == null || !mainWindow.IsLoaded)
                {
                    Debug.WriteLine("主窗口未加载，延迟注册热键");

                    // 延迟注册
                    mainWindow?.Dispatcher.InvokeAsync(() =>
                    {
                        if (mainWindow.IsLoaded)
                        {
                            RegisterInternal();
                        }
                    }, DispatcherPriority.Background);

                    return false;
                }

                return RegisterInternal();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"热键注册异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 内部注册方法
        /// </summary>
        private bool RegisterInternal()
        {
            var window = System.Windows.Application.Current.MainWindow;
            if (window == null) return false;

            var windowHelper = new WindowInteropHelper(window);
            var handle = windowHelper.Handle;

            // 确保窗口句柄有效
            if (handle == IntPtr.Zero)
            {
                Debug.WriteLine("窗口句柄无效，无法注册热键");
                return false;
            }

            // 转换Key到虚拟键码
            var virtualKey = KeyInterop.VirtualKeyFromKey(_key);

            // 注册热键
            var isSuccess = RegisterHotKey(handle, _id, (uint)_modifiers, (uint)virtualKey);

            if (isSuccess)
            {
                // 添加消息钩子
                ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessage;
                _isRegistered = true;

                Debug.WriteLine($"热键注册成功 - ID: {_id}, Key: {_key}, Modifiers: {_modifiers}");
            }
            else
            {
                // 获取错误代码
                var errorCode = Marshal.GetLastWin32Error();
                Debug.WriteLine($"热键注册失败 - 错误代码: 0x{errorCode:X8}");
            }

            return isSuccess;
        }

        /// <summary>
        /// 线程消息预处理
        /// </summary>
        private void ThreadPreprocessMessage(ref System.Windows.Interop.MSG msg, ref bool handled)
        {
            if (handled) return;
            if (msg.message != WM_HOTKEY) return;
            if ((int)msg.wParam != _id) return;

            // 执行热键动作
            try
            {
                _action?.Invoke(this);
                handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"热键处理异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消注册热键
        /// </summary>
        public void Unregister()
        {
            try
            {
                if (!_isRegistered) return;

                // 获取主窗口句柄
                var window = System.Windows.Application.Current?.MainWindow;
                if (window != null)
                {
                    var windowHelper = new WindowInteropHelper(window);
                    var handle = windowHelper.Handle;

                    if (handle != IntPtr.Zero)
                    {
                        UnregisterHotKey(handle, _id);
                    }
                }

                // 移除消息钩子
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessage;
                _isRegistered = false;

                Debug.WriteLine($"热键取消注册 - ID: {_id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"热键取消注册异常: {ex.Message}");
            }
        }


        #region Windows API

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

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
                }

                // 清理非托管资源
                Unregister();

                _disposed = true;
            }
        }

        ~HotKey()
        {
            Dispose(false);
        }

        #endregion

        #region 属性

        public Key Key => _key;
        public ModifierKeys Modifiers => _modifiers;
        public bool IsRegistered => _isRegistered;

        #endregion
    }
}