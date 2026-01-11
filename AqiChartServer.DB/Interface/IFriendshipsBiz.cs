using AqiChart.Model.Dto;
using AqiChartServer.DB.Enums;

namespace AqiChartServer.DB.Interface
{
    public interface IFriendshipsBiz
    {
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<UserDto> GetFriends(string userId);

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="userId">发起用户</param>
        /// <param name="friendId">要添加的好友</param>
        /// <returns></returns>
        bool AddFriend(string userId, string friendId);

        /// <summary>
        /// 同意并通过添加好友
        /// </summary>
        /// <param name="userId">同意用户</param>
        /// <param name="friendId">发起好友用户</param>
        /// <returns></returns>
        bool ThroughFriend(string userId, string friendId);

        /// <summary>
        /// 拒绝添加好友
        /// </summary>
        /// <param name="userId">拒绝人</param>
        /// <param name="friendId">被拒绝的用户</param>
        /// <returns></returns>
        bool RejectFriend(string userId, string friendId);

        /// <summary>
        /// 查询好友列表
        /// </summary>
        /// <param name="userId">用户</param>
        /// <param name="status">好友状态</param>
        /// <returns></returns>
        List<UserDto> GetFriendList(string userId, FriendshipsStatusEnum status);
    }
}
