using System.Windows.Input;
using System.Windows;
using WeChat.Client.ScreenshotTool;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AqiChart.Client.ScreenshotTool
{
    /// <summary>
    /// 截图服务（单例模式）
    /// 管理全局热键和截图窗口
    /// </summary>
    public class ScreenshotService : IDisposable
    {
        private static readonly Lazy<ScreenshotService> _instance =
            new Lazy<ScreenshotService>(() => new ScreenshotService());

        public static ScreenshotService Instance => _instance.Value;

        private HotKey? _hotKey;
        private Action<object, BitmapSource>? _onScreenshotCaptured;
        private ScreenshotWindow? _currentWindow;
        private bool _isDisposed;

        // 热键配置
        public Key HotKey { get; set; } = Key.A;
        public ModifierKeys HotKeyModifiers { get; set; } = ModifierKeys.Control | ModifierKeys.Alt;

        /// <summary>
        /// 截图完成事件
        /// </summary>
        public event EventHandler<BitmapSource>? ScreenshotCaptured;

        /// <summary>
        /// 热键触发事件
        /// </summary>
        public event EventHandler? HotKeyPressed;

        private ScreenshotService()
        {
            InitializeHotKey();
        }

        /// <summary>
        /// 初始化全局热键
        /// </summary>
        private void InitializeHotKey()
        {
            try
            {
                // 清理可能存在的旧热键
                _hotKey?.Dispose();

                // 创建新的热键
                _hotKey = new HotKey(HotKey, HotKeyModifiers, OnHotKeyPressed);

                if (_hotKey.Register())
                {
                    Debug.WriteLine($"热键注册成功: {HotKeyModifiers}+{HotKey}");
                }
                else
                {
                    Debug.WriteLine("热键注册失败，将使用备用热键");
                    TryAlternativeHotKeys();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"热键初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 尝试备用热键
        /// </summary>
        private void TryAlternativeHotKeys()
        {
            var alternativeHotKeys = new[]
            {
                (Key.S, ModifierKeys.Control | ModifierKeys.Alt),
                (Key.D, ModifierKeys.Control | ModifierKeys.Alt),
                (Key.F1, ModifierKeys.Control | ModifierKeys.Alt),
                (Key.PrintScreen, ModifierKeys.Control | ModifierKeys.Shift),
            };

            foreach (var (key, modifiers) in alternativeHotKeys)
            {
                try
                {
                    _hotKey?.Dispose();
                    _hotKey = new HotKey(key, modifiers, OnHotKeyPressed);

                    if (_hotKey.Register())
                    {
                        HotKey = key;
                        HotKeyModifiers = modifiers;
                        Debug.WriteLine($"备用热键注册成功: {modifiers}+{key}");
                        return;
                    }
                }
                catch
                {
                    // 继续尝试下一个热键
                }
            }

            Debug.WriteLine("所有热键注册失败");
        }

        /// <summary>
        /// 热键按下事件处理
        /// </summary>
        private void OnHotKeyPressed(HotKey hotKey)
        {
            // 触发热键事件
            HotKeyPressed?.Invoke(this, EventArgs.Empty);

            // 在主线程显示截图窗口
            Application.Current.Dispatcher.Invoke(() =>
            {
                CaptureScreenshot(null);
            });
        }

        /// <summary>
        /// 重新注册热键
        /// </summary>
        public bool ReRegisterHotKey(Key key, ModifierKeys modifiers)
        {
            try
            {
                HotKey = key;
                HotKeyModifiers = modifiers;

                _hotKey?.Dispose();
                _hotKey = new HotKey(key, modifiers, OnHotKeyPressed);

                return _hotKey.Register();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"重新注册热键失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 捕获屏幕截图
        /// </summary>
        /// <param name="onCaptured">截图完成回调</param>
        public void CaptureScreenshot(Action<object, BitmapSource>? onCaptured = null)
        {
            try
            {
                // 如果已有截图窗口，先关闭
                _currentWindow?.Close();
                _currentWindow = null;

                // 设置回调
                _onScreenshotCaptured = onCaptured;

                // 创建新的截图窗口
                _currentWindow = new ScreenshotWindow();
                _currentWindow.ScreenshotTaken += OnScreenshotTaken;
                _currentWindow.ScreenshotCancelled += OnScreenshotWindowClosed;
                _currentWindow.Show();

                Debug.WriteLine("截图窗口已显示");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"启动截图失败: {ex.Message}");
                MessageBox.Show($"启动截图失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnScreenshotTaken(object sender, BitmapSource screenshot)
        {
            try
            {
                // 调用回调函数
                _onScreenshotCaptured?.Invoke(sender, screenshot);

                // 触发事件
                ScreenshotCaptured?.Invoke(this, screenshot);

                Debug.WriteLine($"截图完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"截图完成回调处理失败: {ex.Message}");
            }
            finally
            {
                _currentWindow = null;
            }
        }


        /// <summary>
        /// 截图窗口关闭事件
        /// </summary>
        private void OnScreenshotWindowClosed(object? sender, EventArgs e)
        {
            _currentWindow = null;
        }

        /// <summary>
        /// 获取当前热键描述
        /// </summary>
        public string GetHotKeyDescription()
        {
            return $"{HotKeyModifiers}+{HotKey}";
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                _hotKey?.Dispose();
                _currentWindow?.Close();
                _currentWindow = null;
            }
            catch
            {
                // 忽略释放过程中的错误
            }

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        ~ScreenshotService()
        {
            Dispose();
        }
    }
}
