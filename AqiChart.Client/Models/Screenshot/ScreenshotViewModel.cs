using AqiChart.Client.ScreenshotTool;
using Caliburn.Micro;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace AqiChart.Client.Models.Screenshot
{
    public class ScreenshotViewModel : Screen, IChildViewModel
    {

        private ScreenshotWindow _screenshotWindow;
        private List<BitmapSource> _screenshots = new List<BitmapSource>();

        public string PageName { get; set; } = "Screenshot";

        public Window _mainView;
        public ScreenshotViewModel()
        {
            _mainView = AppBootstrapper.MainView;
        }


        private string textStatus = "就绪";
        public string TextStatus
        {
            get => textStatus;
            set
            {
                textStatus = value;
                this.NotifyOfPropertyChange(() => TextStatus);
            }
        }

        private string hotKey = "Ctrl+Alt+A";
        public string HotKey
        {
            get => hotKey;
            set
            {
                hotKey = value;
                this.NotifyOfPropertyChange(() => HotKey);
            }
        }
        private string screenInfo = "正在获取屏幕信息...";
        public string ScreenInfo
        {
            get => screenInfo;
            set
            {
                screenInfo = value;
                this.NotifyOfPropertyChange(() => ScreenInfo);
            }
        }

        public void StartScreenshot()
        {
            try
            {
                
                // 隐藏主窗口
                this._mainView.WindowState = WindowState.Minimized;

                // 延迟一小段时间确保窗口最小化
                Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    // 关闭可能存在的截图窗口
                    _screenshotWindow?.Close();

                    // 创建新的截图窗口
                    _screenshotWindow = new ScreenshotWindow();
                    _screenshotWindow.ScreenshotTaken += OnScreenshotTaken;
                    _screenshotWindow.ScreenshotCancelled += OnScreenshotCancelled;
                    _screenshotWindow.Closed += (s, args) => _screenshotWindow = null;

                    _screenshotWindow.Show();

                    this._view.StatusText.Text = "截图模式已激活 - 拖动鼠标选择区域";
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动截图失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnScreenshotTaken(object sender, BitmapSource screenshot)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 保存截图
                _screenshots.Add(screenshot);

                // 恢复主窗口
                this._mainView.WindowState = WindowState.Normal;
                this._mainView.Activate();

                // 添加截图预览
                AddScreenshotPreview(screenshot);

                this._view.StatusText.Text = "截图完成 - 已添加到历史记录";

                // 自动复制到剪贴板
                try
                {
                    Clipboard.SetImage(screenshot);
                    this._view.StatusText.Text = "截图完成 - 已复制到剪贴板并添加到历史记录";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"复制到剪贴板失败: {ex.Message}");
                }
            });
        }

        private void OnScreenshotCancelled(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 恢复主窗口
                this._mainView.WindowState = WindowState.Normal;
                this._mainView.Activate();

                this._view.StatusText.Text = "截图已取消";
            });
        }

        private void AddScreenshotPreview(BitmapSource screenshot)
        {

            // 创建截图项容器
            var screenshotItem = new Border
            {
                Style = (Style)this._view.FindResource("ScreenshotItemStyle")
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 截图预览
            var image = new Image
            {
                Source = screenshot,
                Stretch = Stretch.Uniform,
                MaxHeight = 180,
                Margin = new Thickness(15)
            };

            Grid.SetRow(image, 0);
            grid.Children.Add(image);

            // 操作按钮
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            var copyButton = new Button
            {
                Content = "📋 复制",
                Style = (Style)this._view.FindResource("ActionButtonStyle"),
                Margin = new Thickness(5),
                Tag = screenshot,
                ToolTip = "复制到剪贴板"
            };
            copyButton.Click += (s, e) => CopyScreenshotToClipboard((BitmapSource)((Button)s).Tag);

            var saveButton = new Button
            {
                Content = "💾 保存",
                Style = (Style)this._view.FindResource("ActionButtonStyle"),
                Margin = new Thickness(5),
                Tag = screenshot,
                ToolTip = "保存为文件"
            };
            saveButton.Click += (s, e) => SaveScreenshotToFile((BitmapSource)((Button)s).Tag);

            var viewButton = new Button
            {
                Content = "👁 查看",
                Style = (Style)this._view.FindResource("ActionButtonStyle"),
                Margin = new Thickness(5),
                Tag = screenshot,
                ToolTip = "查看大图"
            };
            viewButton.Click += (s, e) => ShowScreenshotPreview((BitmapSource)((Button)s).Tag);

            var deleteButton = new Button
            {
                Content = "🗑 删除",
                Style = (Style)this._view.FindResource("ActionButtonStyle"),
                Margin = new Thickness(5),
                Tag = screenshotItem,
                ToolTip = "从历史记录中删除"
            };
            deleteButton.Click += (s, e) =>
            {
                var item = (Border)((Button)s).Tag;
                this._view.ScreenshotListPanel.Children.Remove(item);
                _screenshots.Remove(screenshot);
                UpdateHistoryCount();

                if (this._view.ScreenshotListPanel.Children.Count == 0)
                {
                    this._view.EmptyState.Visibility = Visibility.Visible;
                }
            };

            buttonPanel.Children.Add(copyButton);
            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(viewButton);
            buttonPanel.Children.Add(deleteButton);

            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            screenshotItem.Child = grid;

            // 双击查看大图
            screenshotItem.MouseDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    ShowScreenshotPreview(screenshot);
                }
            };

            // 添加到列表顶部
            this._view.ScreenshotListPanel.Children.Insert(0, screenshotItem);
            UpdateHistoryCount();
        }

        private void UpdateHistoryCount()
        {
            this._view.HistoryCountText.Text = $"({_screenshots.Count})";
        }

        private void ShowScreenshotPreview(BitmapSource screenshot)
        {
            var previewWindow = new Window
            {
                Title = "截图预览",
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this._mainView,
                WindowStyle = WindowStyle.SingleBorderWindow,
                Background = Brushes.White
            };

            var dockPanel = new DockPanel();

            // 工具栏
            var toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            DockPanel.SetDock(toolbar, Dock.Top);

            var copyButton = new Button
            {
                Content = "复制",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5),
                Tag = screenshot
            };
            copyButton.Click += (s, e) => CopyScreenshotToClipboard((BitmapSource)((Button)s).Tag);

            var saveButton = new Button
            {
                Content = "保存",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5),
                Tag = screenshot
            };
            saveButton.Click += (s, e) => SaveScreenshotToFile((BitmapSource)((Button)s).Tag);

            var closeButton = new Button
            {
                Content = "关闭",
                Margin = new Thickness(5),
                Padding = new Thickness(10, 5, 10, 5)
            };
            closeButton.Click += (s, e) => previewWindow.Close();

            toolbar.Children.Add(copyButton);
            toolbar.Children.Add(saveButton);
            toolbar.Children.Add(closeButton);

            // 图像显示区域
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var image = new Image
            {
                Source = screenshot,
                Stretch = Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };

            scrollViewer.Content = image;

            dockPanel.Children.Add(toolbar);
            dockPanel.Children.Add(scrollViewer);

            previewWindow.Content = dockPanel;
            previewWindow.ShowDialog();
        }

        private void CopyScreenshotToClipboard(BitmapSource screenshot)
        {
            try
            {
                Clipboard.SetImage(screenshot);
                this._view.StatusText.Text = "已复制到剪贴板";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveScreenshotToFile(BitmapSource screenshot)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG 图片|*.png|JPEG 图片|*.jpg|BMP 图片|*.bmp",
                DefaultExt = "png",
                FileName = $"截图_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string extension = Path.GetExtension(saveDialog.FileName).ToLower();
                    BitmapEncoder encoder;

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(screenshot));

                    using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    this._view.StatusText.Text = $"已保存到: {Path.GetFileName(saveDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region System

        private ScreenshotView _view;

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _view = view as ScreenshotView;
        }

        #endregion



    }
}
