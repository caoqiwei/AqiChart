using AqiChart.Client.Data;
using AqiChart.Client.Services;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AqiChart.Client.Models.Chat
{
    public class ChatViewModel : Screen, IChatViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        public ChatViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            //IEventAggregator eventAggregator = IoC.Get<IEventAggregator>();
            //_eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);
            
        }

        private ObservableCollection<ChatContent> _chats = new ObservableCollection<ChatContent>();
        public ObservableCollection<ChatContent> Chats { 
            get => _chats; 
            set { 
                _chats = value; 
                NotifyOfPropertyChange(() => Chats); 
            } 
        }

        public string UserId { get; set; } = string.Empty;

        public UserDto Friend { get; set; } = new UserDto();

        private string _message = string.Empty;
        public string Message { get => _message; set { _message = value; NotifyOfPropertyChange(() => Message); } }

        public async Task SendMessage()
        {
            if (string.IsNullOrEmpty(Message)) return;
            await _eventAggregator.PublishOnUIThreadAsync(new UserSendMessage() { UserId = UserId, Message = Message });
            Message = string.Empty;
        }

        public async Task MessageEnter(KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        #region System Actions

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializedAsync(cancellationToken);
        }
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            return base.OnActivatedAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            return base.TryCloseAsync(dialogResult);
        }
        #endregion

        public void AddChat(UserReceiveMessage message)
        {
            if (message == null) return;
            ChatContent chatContent = new ChatContent()
            {
                Id = message.Id,
                Content = message.Message,
                DateTime = message.Time,
                IsMe = message.IsMe,
                NickName = message.NickName,
                UserId = message.UserId,
                AvatarUrl = message.AvatarUrl,
                Type = message.Type
            };
            Chats.Add(chatContent);

            ///单条信息已读；
            SetReadById(message.Id);

        }

        private async void SetReadById(string id)
        {
            _= await ApiService.SetReadById(id);
        }

        public void AddChats(List<UserReceiveMessage> messages)
        {
            if (messages == null|| messages.Count == 0) return;
            foreach (var message in messages)
            {
                ChatContent chatContent = new ChatContent()
                {
                    Id = message.Id,
                    Content = message.Message,
                    DateTime = message.Time,
                    IsMe = message.IsMe,
                    NickName = message.NickName,
                    UserId = message.UserId,
                    AvatarUrl = message.AvatarUrl,
                    Type = message.Type
                };
                Chats.Add(chatContent);
            }

            ///全部已读；
            ReadAllByUser();
        }

        private async void ReadAllByUser()
        {
            _ = await ApiService.SetReadByFriendId(UserId);
        }


        //public Task HandleAsync(UserReceiveMessage message, CancellationToken cancellationToken)
        //{
        //    if (message.ChartId == UserId)
        //    {
        //        ChatContent chatContent = new ChatContent()
        //        {
        //            Id = message.Id,
        //            Content = message.Message,
        //            DateTime = message.Time,
        //            IsMe = message.IsMe,
        //            NickName = message.NickName,
        //            UserId = message.UserId,
        //            AvatarUrl = message.AvatarUrl,
        //            Type = message.Type
        //        };
        //        Chats.Add(chatContent);
        //    }
        //    return Task.CompletedTask;
        //}


    }
}
