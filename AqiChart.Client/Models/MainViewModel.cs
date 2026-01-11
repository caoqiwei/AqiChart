using AqiChart.Client.Common;
using AqiChart.Client.Data;
using AqiChart.Client.Models.AddressBook;
using AqiChart.Client.Models.Chat;
using AqiChart.Client.Models.Screenshot;
using AqiChart.Client.Models.TestChat;
using AqiChart.Client.ScreenshotTool;
using AqiChart.Client.Services;
using AqiChart.Model.Dto;
using AqiChart.Model.SignalR;
using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AqiChart.Client.Models
{
    public class MainViewModel : Conductor<IChildViewModel>.Collection.OneActive, IShell, IHandle<UserSendMessage>, IHandle<int>
    {
        public readonly IEventAggregator _eventAggregator;

        private BackgroundPollingService _pollingService;

        public Dictionary<string, IChildViewModel> Munus = new Dictionary<string, IChildViewModel>();


        //private readonly ScreenshotService _screenshotService;

        private IChildViewModel GetMunu(string key)
        {
            IChildViewModel model = null;
            if (Munus.ContainsKey(key))
            {
                model = Munus[key];
            }
            else {
                switch (key)
                {
                    case "ChatManage":
                        model = IoC.Get<ChatManageViewModel>();
                        break;
                    case "TestChat":
                        model = IoC.Get<TestChatViewModel>();
                        break;
                    case "Screenshot":
                        model = IoC.Get<ScreenshotViewModel>();
                        break;
                    case "AddressBook":
                        model = IoC.Get<AddressBookViewModel>();
                        break;
                }
                if (model != null) {
                    Munus[key] = model;
                    Items.Add(model);
                } 

            }
            return model;
        }

        public UserModel User { get; set; }
        public MainViewModel(IEventAggregator eventAggregator)
        {
            LogHelper.Info($"Login in MainView");
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);
            User = SettingConfig.User;

            // 初始化
            //_screenshotService = ScreenshotService.Instance;

            InitializeHubConnection();
            InitializePollingService();
            InitializeTaskbarIcon();
            //IChildViewModel model = IoC.Get<ChatManageViewModel>();
            //Munus[model.PageName] = model;
            IChildViewModel model = GetMunu("ChatManage");
            this.ActivateItemAsync(model);

        }

        public async Task SwitchView(string o)
        {
            IChildViewModel model = GetMunu(o);
            if (model != null)
            {
                await this.ActivateItemAsync(model);
            }
        }

        private int chartCount = 0;
        public int ChartCount
        { 
            get=> chartCount; 
            set {
                chartCount = value; 
                this.NotifyOfPropertyChange(() => ChartCount);
            } 
        }


        #region 托盘图标
        private TaskbarIcon? _taskbarIcon;
        private bool _isClosing = false;

        private void InitializeTaskbarIcon()
        {
            try
            {
                ImageSource iconSource = new BitmapImage(new Uri("pack://application:,,,/AqiChart.Client;component/Resources/favicon.ico", UriKind.Absolute));
                // 创建托盘图标
                _taskbarIcon = new TaskbarIcon
                {
                    IconSource = iconSource,
                    ToolTipText = "七聊\n左键双击显示窗口\n右键单击显示菜单",

                    // 托盘图标点击事件
                    DoubleClickCommand = new RelayCommand(ShowWindowCommand)
                };

                // 创建托盘菜单
                CreateTrayMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化托盘图标失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateTrayMenu()
        {
            if (_taskbarIcon == null) return;

            var menu = new System.Windows.Controls.ContextMenu();

            // 显示窗口
            var showMenuItem = new System.Windows.Controls.MenuItem
            {
                Header = "显示窗口",
                Icon = CreateMenuIcon("M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z")
            };
            showMenuItem.Click += (s, e) => ShowWindowCommand();

            // 隐藏窗口
            var hideMenuItem = new System.Windows.Controls.MenuItem
            {
                Header = "隐藏窗口",
                Icon = CreateMenuIcon("M20,12A8,8 0 0,0 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12M22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2A10,10 0 0,1 22,12M15.4,16.6L10.8,12L15.4,7.4L14,6L8,12L14,18L15.4,16.6Z")
            };
            hideMenuItem.Click += (s, e) => HideWindow();

            // 分隔线
            menu.Items.Add(new System.Windows.Controls.Separator());


            // 退出程序
            var exitMenuItem = new System.Windows.Controls.MenuItem
            {
                Header = "退出",
                Icon = CreateMenuIcon("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z")
            };
            exitMenuItem.Click += (s, e) => ExitApplication();

            // 添加菜单项
            menu.Items.Add(showMenuItem);
            menu.Items.Add(hideMenuItem);
            menu.Items.Add(new System.Windows.Controls.Separator());
            menu.Items.Add(exitMenuItem);

            _taskbarIcon.ContextMenu = menu;
        }

        private System.Windows.Controls.Viewbox CreateMenuIcon(string pathData)
        {
            var viewbox = new System.Windows.Controls.Viewbox
            {
                Width = 16,
                Height = 16,
                Child = new System.Windows.Shapes.Path
                {
                    Data = Geometry.Parse(pathData),
                    Fill = Brushes.Black,
                    Stretch = Stretch.Uniform
                }
            };

            return viewbox;
        }

        public void ShowWindowCommand()
        {
            this._view.Visibility = Visibility.Visible;
            //this._view.Topmost = true;
            this._view.WindowState = WindowState.Normal;
        }

        public void HideWindow()
        {
            this._view.Visibility = Visibility.Hidden;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            HideWindow();

            // 显示通知
            ShowTrayNotification("窗口已最小化", "应用程序已最小化到系统托盘，双击托盘图标可恢复显示。");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }

        private void btnSendNotification_Click(object sender, RoutedEventArgs e)
        {
            SendTestNotification();
        }

        private void btnOpenLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TrayApp",
                    "logs");

                if (Directory.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                }
                else
                {
                    MessageBox.Show("日志目录不存在。", "信息",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开日志目录失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendTestNotification()
        {
            ShowTrayNotification("测试通知", "这是一条测试通知消息，来自系统托盘应用。");
        }

        private void ShowTrayNotification(string title, string message)
        {
            if (_taskbarIcon != null)
            {
                _taskbarIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
            }
        }

        private void ShowAboutDialog()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var message = $"七聊\n\n" +
                         $"版本: {version}\n" +
                         $"框架: .NET 8.0\n" +
                         $"这是一个演示在系统托盘显示图标的WPF应用程序。";

            MessageBox.Show(message, "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = true; // 取消关闭，改为隐藏
                HideWindow();
            }
        }

        private void ExitApplication()
        {
            _isClosing = true;

            // 清理托盘图标
            if (_taskbarIcon != null)
            {
                _taskbarIcon.Dispose();
            }

            // 退出应用程序
            Application.Current.Shutdown();
        }

        #endregion


        #region 后台轮询任务

        private void InitializePollingService()
        {
            // 配置轮询任务
            var config = new PollingTaskConfig
            {
                Interval = TimeSpan.FromMinutes(5),
                RetryInterval = TimeSpan.FromMinutes(1),
                MaxRetryCount = 3,
                RunImmediately = true,
                AutoStart = false
            };

            // 创建轮询服务
            _pollingService = new BackgroundPollingService(ExecuteHeartbeatTaskAsync, config);

            // 订阅事件
            //_pollingService.TaskStarted += OnTaskStarted;
            //_pollingService.TaskCompleted += OnTaskCompleted;
            //_pollingService.TaskFailed += OnTaskFailed;
            //_pollingService.StatusChanged += OnStatusChanged;
            //_pollingService.ServiceStarted += OnServiceStarted;
            //_pollingService.ServiceStopped += OnServiceStopped;

            // 更新UI状态
        }

        private async Task<PollingTaskResult> ExecuteHeartbeatTaskAsync(CancellationToken cancellationToken)
        {
            // 模拟异步任务
            //await Task.Delay(1000, cancellationToken);

            // 这里执行实际的业务逻辑
            // 例如：检查服务状态、发送心跳包、同步数据等

            var isSuccess = await CheckServiceStatusAsync();

            return new PollingTaskResult
            {
                IsSuccess = isSuccess,
                Message = isSuccess ? "心跳检测成功" : "心跳检测失败",
                ExecutionTime = DateTime.Now
            };
        }

        private async Task<bool> CheckServiceStatusAsync()
        {
            // 模拟检查服务状态
            var isOK =  await ApiService.UpdateLastUserTime();
            return isOK; // 90%的成功率
        }

        #endregion


        #region System Button 

        private MainView _view;

        private bool isMaxWindow;
        public bool IsMaxWindow
        {
            get => isMaxWindow;
            set
            {
                isMaxWindow = value;
                this.NotifyOfPropertyChange(() => IsMaxWindow);
            }
        }

        public void MaxWindow()
        {
            if (_view.WindowState == WindowState.Maximized)
            {
                IsMaxWindow = false;
                _view.WindowState = WindowState.Normal;
            }
            else
            {
                IsMaxWindow = true;
                _view.WindowState = WindowState.Maximized;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _view = view as MainView;
            AppBootstrapper.MainView = _view;
            //截图
            RegisterGlobalHotkey();
        }
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializedAsync(cancellationToken);
        }
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            _ = _pollingService.StartAsync(); // 启动后台轮询任务
            return base.OnActivatedAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_hubConnection != null)
            {
                _hubConnection.StopAsync();
                _hubConnection.DisposeAsync();
            }
            _eventAggregator.Unsubscribe(this);

            _ = _pollingService.StopAsync();

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion


        #region SignalR
        private HubConnection _hubConnection;

        private async void InitializeHubConnection()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(SettingConfig.SignalRUrl, options =>
                {
                    options.Headers["Authorization"] = $"Bearer {SettingConfig.Token}";
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ReceiveMessage>("ReceiveMessage", async (message) =>
                {
                    var model = new UserReceiveMessage()
                    {
                        Id = message.Id,
                        ChartId = message.SenderId,
                        UserId = message.SenderId,
                        NickName = message.NickName,
                        AvatarUrl = message.AvatarUrl,
                        IsMe = false,
                        Time = message.SentAt,
                        Message = message.Content,
                        Type = message.ContentType
                    };
                    // 方式1：使用 Dispatcher
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await _eventAggregator.PublishOnUIThreadAsync(model);
                    });

                    //// 方式2：使用 Caliburn.Micro 的 Execute
                    //await Execute.OnUIThreadAsync(async () =>
                    //{
                    //    await _eventAggregator.PublishOnUIThreadAsync(model);
                    //});

                });
            _hubConnection.On<SentMeMessage>("SentMe", async (message) =>
                {
                    var model = new UserReceiveMessage()
                    {
                        Id = message.Id,
                        ChartId = message.ReceiverId,
                        UserId = User.Id,
                        NickName = User.NickName,
                        AvatarUrl = User.AvatarUrl,
                        Message = message.Content,
                        IsMe = true,
                        Time = message.SentAt,
                        Type = message.ContentType
                    };
                    // 方式1：使用 Dispatcher
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await _eventAggregator.PublishOnUIThreadAsync(model);
                    });

                    //// 方式2：使用 Caliburn.Micro 的 Execute
                    //await Execute.OnUIThreadAsync(async () =>
                    //{
                    //    await _eventAggregator.PublishOnUIThreadAsync(model);
                    //});

                });

            //_hubConnection.Reconnected += async (connectionId) =>
            //{
            //    LogHelper.Info($"Reconnected: {connectionId}");
            //};

            await _hubConnection.StartAsync();
        }


        private async Task Send(string receiver, string messge)
        {
            PrivateChatDto dto = new PrivateChatDto()
            {
                Content = messge,
                ReceiverId = receiver,
                SenderId = User.Id,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid().ToString()
            };

            await _hubConnection.InvokeAsync("SendMessageToFriend", dto.ReceiverId, dto.Content);
        }

        #endregion


        #region TcpClient
        private TcpClient client;
        private NetworkStream stream;
        private string nickname;
        public ObservableCollection<string> Outputs { get; set; } = new ObservableCollection<string>();

        private string msg;
        public string Msg
        {
            get => msg;
            set
            {
                if (msg != value)
                {
                    msg = value;
                    NotifyOfPropertyChange(() => Msg);
                }
            }
        }


        public void ConnectServer()
        {
            try
            {
                if (string.IsNullOrEmpty(msg)) return;
                nickname = msg;
                string ip = "127.0.0.1";

                client = new TcpClient(ip, 8888);
                stream = client.GetStream();

                // 发送昵称
                byte[] nameData = Encoding.UTF8.GetBytes(nickname);
                stream.Write(nameData, 0, nameData.Length);
                Msg = "";
                // 启动接收线程
                Thread receiveThread = new Thread(ReceiveMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                Environment.Exit(1);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    WriteOutput(message);

                }
                ///Debug.WriteLine("3333");
            }
            catch { /* 正常断开 */ }
        }

        public void Send()
        {
            if (!string.IsNullOrEmpty(msg))
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
            Msg = "";
        }


        private void WriteOutput(string message, string user = null)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                Outputs.Add(message);
            });
        }

        #endregion

        public void TestHandle()
        {
            _eventAggregator.PublishOnUIThreadAsync(new UserReceiveMessage()
            {
                Id = "testid",
                ChartId = "testchartid",
                UserId = User.Id,
                NickName = User.NickName,
                AvatarUrl = User.AvatarUrl,
                IsMe = true,
            });
        }

        public Task HandleAsync(UserSendMessage message, CancellationToken cancellationToken)
        {
            Send(message.UserId, message.Message);
            return Task.CompletedTask;
        }

        public Task HandleAsync(int message, CancellationToken cancellationToken)
        {
            ChartCount = message;
            return Task.CompletedTask;
        }


        private GlobalHotkey _globalHotkey;
        private ScreenshotWindow _screenshotWindow;
        private void RegisterGlobalHotkey()
        {
            try
            {
                // 注册Ctrl+Alt+A作为截图快捷键
                _globalHotkey = new GlobalHotkey(this._view, 9000,
                    ModifierKeys.Control | ModifierKeys.Alt, Key.A);

                _globalHotkey.HotkeyPressed += (s, args) =>
                {
                    // 在UI线程中执行截图
                    Application.Current.Dispatcher.Invoke(() => StartScreenshot());
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法注册全局快捷键: {ex.Message}\n" +
                    "截图功能将无法通过快捷键使用。",
                    "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        public void StartScreenshot()
        {
            try
            {

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
                // 自动复制到剪贴板
                try
                {
                    Clipboard.SetImage(screenshot);
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
                this._view.WindowState = WindowState.Normal;

            });
        }



    }
}
