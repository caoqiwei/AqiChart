using AqiChart.Client.Data;
using AqiChart.Client.Services;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace AqiChart.Client.Models.TestChat
{
    public class TestChatViewModel : Screen, IChildViewModel
    {
        public string PageName { get; set; } = "TestChat";

        public ObservableCollection<UserDto> FriendList { get; set; } = new ObservableCollection<UserDto>();
        private readonly IEventAggregator _eventAggregator;

        

        public TestChatViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            GetFriends();

            #region 在任意位置获取容器实例
            //// 在任意位置获取容器实例
            //var container = (SimpleContainer)IoC.GetInstance(typeof(IServiceProvider), null);
            //// 动态注册新 ViewModel
            //container.RegisterPerRequest(typeof(ChatViewModel), null, typeof(ChatViewModel));

            //// 在任意位置获取ViewModel实例,如果返回Null 则表示没有注册
            //var loginVM = IoC.Get<LoginViewModel>();
            #endregion
        }

        private void GetFriends()
        {
            Task.Run(async () =>
            {
                var users = await ApiService.GetFriends();
                Debug.WriteLine("User Count:" + users.Count);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FriendList = new ObservableCollection<UserDto>(users);
                });
            });

        }



    }
}
