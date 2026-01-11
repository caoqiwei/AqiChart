using AqiChart.Model.SignalR;
using AqiChart.Model.Dto;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using AqiChartServer.DB.Interface;
using Serilog;

namespace AqiChartServer.WebApi.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        
        private readonly IUserBiz _userBiz;
        private readonly IPrivateChatBiz _privateChatBiz;
        public ChatHub(IPrivateChatBiz privateChatBiz, IUserBiz userBiz)
        {
            _privateChatBiz = privateChatBiz;
            _userBiz = userBiz;
        }

        public async Task SendMessage(string id,string receiverId, string message) 
        {
            //PrivateChatDto dto;
            //string receiverId, string message
            //new PrivateChatDto { SenderId = senderId, ReceiverId = receiverId, Content = message }
            var senderId = Context.User.Identity.Name;
            var dto = new PrivateChatDto { Id =id, SenderId = senderId, ContentType= ContentType.text.ToString(), ReceiverId = receiverId, Content = message };
            _privateChatBiz.AddPrivateChats(dto);
            await Clients.User(dto.ReceiverId).SendAsync("ReceiveMessage", senderId, dto.Content, DateTime.Now);
        }

        public async Task<bool> RegisterUserConnection(string userId)
        {
            try
            {
                var user = _userBiz.GetUserInfo(userId);
                if (user == null) return false;

                _userBiz.UpdateUserStatus(userId, UserStatus.online);
                _userConnections[userId] = Context.ConnectionId;

                //用户好友列表状态更新
                //var friends = await _userService.GetFriendsAsync(userId);
                //foreach (var friend in friends)
                //{
                //    if (_userConnections.ContainsKey(friend.Id))
                //    {
                //        await Clients.Client(_userConnections[friend.Id]).SendAsync("FriendStatusChanged", userId, true);
                //    }
                //}

                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"注册用户连接失败: {ex.Message}");
                return false;
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Identity.Name;
            var user = _userBiz.GetUserInfo(userId);
            if (user == null) return;

            _userBiz.UpdateUserStatus(userId, UserStatus.online);
            _userConnections[userId] = Context.ConnectionId;

            // 通知好友用户上线
            //var friends = await _userService.GetFriendsAsync(user.Id);
            //foreach (var friend in friends)
            //{
            //    if (_userConnections.ContainsKey(friend.Id))
            //    {
            //        await Clients.Client(_userConnections[friend.Id]).SendAsync("FriendStatusChanged", user.Id, True);
            //    }
            //}

            await base.OnConnectedAsync();
        }

        private string GetUserId(string connectionId)
        {
            string userId = null;
            foreach (var kvp in _userConnections)
            {
                if (kvp.Value == Context.ConnectionId)
                {
                    userId = kvp.Key;
                    break;
                }
            }
            return userId;
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            string userId = GetUserId(Context.ConnectionId);

            if (userId != null)
            {
                _userBiz.UpdateUserStatus(userId, UserStatus.offline);
                _userConnections.TryRemove(userId, out _);

                // 通知好友用户离线
                //var friends = await _userService.GetFriendsAsync(user.Id);
                //foreach (var friend in friends)
                //{
                //    if (_userConnections.ContainsKey(friend.Id))
                //    {
                //        await Clients.Client(_userConnections[friend.Id]).SendAsync("FriendStatusChanged", user.Id, false);
                //    }
                //}
                Log.Information($"客户端断开ConnectionId: {Context.ConnectionId}，用户：{userId}");
            }

            Log.Information($"客户端断开ConnectionId: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToFriend(string receiverId, string message)
        {

            string userId = GetUserId(Context.ConnectionId);
            if (userId == null) return;
            var user = _userBiz.GetUserInfo(userId);
            if (user == null) return;

            var dto = new PrivateChatDto { SenderId = userId, ContentType = ContentType.text.ToString(), ReceiverId = receiverId, Content = message };
            var model = _privateChatBiz.AddPrivateChats(dto);
            if (model == null) return;

            // 发送给接收者（如果在线）
            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", new ReceiveMessage
                {
                    Id = model.MessageId,
                    SenderId = user.UserId,
                    NickName = user.NickName,
                    AvatarUrl = user.AvatarUrl,
                    ContentType = model.ContentType,
                    Content = message,
                    SentAt = model.CreatedAt
                });
            }

            // 也发送给自己（用于确认）
            await Clients.Caller.SendAsync("SentMe", new SentMeMessage
            {
                Id = model.MessageId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = model.CreatedAt,
                ContentType = model.ContentType,
            });
        }

        /// <summary>
        /// 供服务端（如Controller）调用的方法：获取所有在线用户的唯一标识
        /// </summary>
        public static List<string> GetAllOnlineUsers()
        {
            return _userConnections.Values.Distinct().ToList();
        }

    }
}
