using AqiChart.Client.Common;
using AqiChart.Client.Data;
using AqiChart.Client.Models.AddressBook;
using AqiChart.Client.Services;
using AqiChart.Model.Dto;
using Caliburn.Micro;
using System.Windows;
using UserDto = AqiChart.Client.Data.UserDto;


namespace AqiChart.Client.Models.Chat
{
    public class ChatManageViewModel : Conductor<IChatViewModel>.Collection.OneActive, IChildViewModel, IHandle<UserReceiveMessage>
    {
        private bool IsFirstView = true;

        /// <summary>
        /// 好友列表聊天框集合
        /// </summary>
        public Dictionary<string, IChatViewModel> ChatList = new Dictionary<string, IChatViewModel>();
        public string PageName { get; set; } = "ChatManage";

        private List<UserReceiveMessage> _receiveMessages = new List<UserReceiveMessage>();
        public List<UserReceiveMessage> ReceiveMessages {  
            get => _receiveMessages;
            set
            {
                _receiveMessages = value;
                NotifyOfPropertyChange(() => ReceiveMessages);
            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                this.NotifyOfPropertyChange(() => SearchText);
            }
        }


        private void UpdateMessageCount()
        {
            foreach (UserDto item in FriendList)
            {
                item.ChartCount = ReceiveMessages.Where(x => x.ChartId == item.Id).Count();
            }
            _eventAggregator.PublishOnUIThreadAsync(ReceiveMessages.Count);
        }

        private List<UserDto> _friendList;
        public List<UserDto> FriendList
        {
            get => _friendList;
            set
            {
                _friendList = value;
                NotifyOfPropertyChange(() => FriendList);
            }
        }

        private UserDto _selectedFriend;
        public UserDto SelectedFriend
        {
            get => _selectedFriend;
            set
            {
                if (_selectedFriend != value)
                {
                    _selectedFriend = value;
                    NotifyOfPropertyChange(() => SelectedFriend);
                    SwitchFriend(_selectedFriend);
                }
            }
        }

        public void SwitchFriend(UserDto user)
        {
            if (user == null) return;
            if (!ChatList.ContainsKey(user.Id))
            {
                //ChatViewModel chatVM = (ChatViewModel)Activator.CreateInstance(typeof(ChatViewModel));
                //ChatViewModel chatVM = (ChatViewModel)Activator.CreateInstance(typeof(ChatViewModel), new object[] { _eventAggregator });
                var chatVM = IoC.Get<ChatViewModel>();
                //ChatViewModel chatVM = new ChatViewModel();
                chatVM.UserId = user.Id;
                chatVM.Friend = user;
                ChatList.Add(user.Id, chatVM);
                Items.Add(chatVM);
            }
            var messages = ReceiveMessages.Where(x => x.ChartId == user.Id).ToList();
            if(messages!=null && messages.Count > 0)
            {
                var cVM = (ChatViewModel)ChatList[user.Id];
                cVM.AddChats(messages);
                ReceiveMessages.RemoveAll(x => x.ChartId == user.Id);
                UpdateMessageCount();
            }
            this.ActivateItemAsync(ChatList[user.Id]);
        }


        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        public ChatManageViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _windowManager = windowManager;
            _eventAggregator.SubscribeOnUIThread(this);
            GetFriends();

            #region 在任意位置获取容器实例
            //// 在任意位置获取容器实例
            //var container = (SimpleContainer)IoC.GetInstance(typeof(IServiceProvider), null);
            //// 动态注册新 ViewModel
            //container.RegisterPerRequest(typeof(ChatViewModel), null, typeof(ChatViewModel));

            //// 获取ViewModel实例,如果返回Null 则表示没有注册
            //var loginVM = IoC.Get<LoginViewModel>();
            #endregion
        }

        private void GetFriends()
        {

            Application.Current.Dispatcher.Invoke(async () =>
            {
                FriendList = await ApiService.GetFriends();
                if (IsFirstView)
                {
                    GetAllUnreadChart();
                    IsFirstView = false;
                }
            });
        }

        private async void GetAllUnreadChart()
        {
            List<PrivateChatDto> list = await ApiService.GetAllUnreadChart();
            //UserReceiveMessage  SettingConfig
            if (list != null)
            {
                foreach (PrivateChatDto friend in list)
                {
                    var user = FriendList.FirstOrDefault(x => x.Id == friend.SenderId);
                    if (user != null)
                    {
                        ReceiveMessages.Add(new UserReceiveMessage
                        {
                            Id = friend.Id,
                            ChartId = friend.SenderId,
                            UserId = friend.SenderId,
                            AvatarUrl = user.AvatarUrl,
                            Message = friend.Content,
                            NickName = user.NickName,
                            Time = friend.CreatedAt,
                            IsMe = false,
                            Type = friend.ContentType
                        });
                    }
                }
                UpdateMessageCount();
            }

        }

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializedAsync(cancellationToken);
        }
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            GetFriends();
            LogHelper.Info("ChatManageViewModel Activated");
            return base.OnActivatedAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public void AddFriend()
        {
            var vm = IoC.Get<AddFriendViewModel>();
            _windowManager.ShowWindowAsync(vm);
            // _windowManager.ShowDialogAsync(vm);
            // _windowManager.ShowPopupAsync(vm);
        }

        public Task HandleAsync(UserReceiveMessage message, CancellationToken cancellationToken)
        {
            LogHelper.Info($"userName:{message.NickName} Message: {message.Message}");
            if(SelectedFriend!=null && SelectedFriend.Id == message.ChartId)
            {
                var chatVM = (ChatViewModel)ChatList[SelectedFriend.Id];
                chatVM.AddChat(message);
            }
            else
            {
                ReceiveMessages.Add(message);
                UpdateMessageCount();
            }
            return Task.CompletedTask;
        }
    }
}
