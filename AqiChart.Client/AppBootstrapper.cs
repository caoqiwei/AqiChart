namespace AqiChart.Client {
    using AqiChart.Client.Common;
    using AqiChart.Client.HttpClient;
    using AqiChart.Client.Models;
    using AqiChart.Client.Models.AddressBook;
    using AqiChart.Client.Models.Chat;
    using AqiChart.Client.Models.Login;
    using AqiChart.Client.Models.Screenshot;
    using AqiChart.Client.Models.TestChat;
    using Caliburn.Micro;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public class AppBootstrapper : BootstrapperBase {
        SimpleContainer _container;
        public static MainView MainView;
        private readonly ApiClient _client;
        public AppBootstrapper() {
            _client = ApiClient.Instance;
            Initialize();
        }

        protected override void Configure() {
            log4net.Config.XmlConfigurator.Configure();
            _container = new SimpleContainer();
            #region ��������
            var frame = new Frame(); // WPF �� Frame �ؼ�
            _container.Instance(frame);
            _container.Singleton<INavigationService, FrameAdapter>();
            #endregion
            // ע�����
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            // ע�� ViewModel
            // _container.PerRequest<IShell, MainViewModel>();
            // _container.PerRequest<IShell, LoginViewModel>();
            _container.PerRequest<LoginViewModel>();
            _container.Singleton<MainViewModel>();
            _container.PerRequest<ChatManageViewModel>();
            _container.PerRequest<TestChatViewModel>();
            _container.PerRequest<ChatViewModel>();
            _container.PerRequest<ScreenshotViewModel>();
            _container.PerRequest<AddFriendViewModel>();
            _container.PerRequest<AddressBookViewModel>();
            //TestChatViewModel  
            //LoginViewModel
            _client.SetBaseUrl(ConfigurationManager.AppSettings["ApiBaseUrl"]);
            SettingConfig.ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            SettingConfig.SignalRUrl = ConfigurationManager.AppSettings["SignalRUrl"];
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        /// <summary>
        /// ȫ���쳣����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.Exception.Message, e.Exception);
            e.Handled = true;
            base.OnUnhandledException(sender, e);
        }
        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            //DisplayRootViewFor<IShell>();
            DisplayRootViewForAsync<LoginViewModel>();

            LogHelper.Name = "Chart Client UI";
        }

        override protected void OnExit(object sender, EventArgs e)
        {
            _client.Dispose();
            base.OnExit(sender, e);
        }
    }
}