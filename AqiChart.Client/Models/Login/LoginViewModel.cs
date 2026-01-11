using AqiChart.Client.Data;
using AqiChart.Client.HttpClient;
using AqiChart.Client.Services;
using Caliburn.Micro;
using System.Windows;

namespace AqiChart.Client.Models.Login
{
    public class LoginViewModel : Screen, IShell
    {
        private readonly INavigationService _navigationService;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;

        public LoginViewModel(IWindowManager windowManager, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            _windowManager = windowManager;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
        }

        public LoginModel LoginModel { get; set; } = new LoginModel() { Account = "user1", Password = "123456" };

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; this.NotifyOfPropertyChange(nameof(ErrorMessage)); }
        }

        private Visibility _showProgress = Visibility.Collapsed;

        public Visibility ShowProgress
        {
            get { return _showProgress; }
            set
            {
                _showProgress = value;
                this.NotifyOfPropertyChange(nameof(ShowProgress));
            }
        }

        public void Login()
        {
            this.ShowProgress = Visibility.Visible;
            this.ErrorMessage = "";

            if (string.IsNullOrEmpty(LoginModel.Account))
            {
                this.ErrorMessage = "请输入用户名！";
                this.ShowProgress = Visibility.Collapsed;
                return;
            }
            if (string.IsNullOrEmpty(LoginModel.Password))
            {
                this.ErrorMessage = "请输入密码！";
                this.ShowProgress = Visibility.Collapsed;
                return;
            }


            Task.Run(new System.Action(async () =>
            {
                //await Task.Delay(2000); 
                try
                {
                    UserModel user = await ApiService.UserLogin(LoginModel);
                    if (user != null)
                    {
                        SettingConfig.Token = user.Token;
                        SettingConfig.User = user;
                        ApiClient.Instance.SetBearerToken(SettingConfig.Token);
                    }

                    //_navigationService.NavigateToViewModel<MainViewModel>();
                    //this.CanClose();
                    Application.Current.Dispatcher.Invoke(new System.Action( async() =>
                    {
                        var mainVM = IoC.Get<MainViewModel>();
                        await _windowManager.ShowWindowAsync(mainVM);
                        await TryCloseAsync();
                    }));

                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                }
            }));
        }

    }
}
