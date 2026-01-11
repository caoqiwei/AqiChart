

using AqiChart.Model.Dto;
using AqiChartServer.DB.Enties;

namespace AqiChartServer.DB.Interface
{
    public interface IUserBiz
    {
        ChatUsers GetUsers(string username);
        ChatUsers GetUsers(string username, string pwd);
        ChatUsers GetUserInfo(string userId);

        List<UserDto> GetUserList();
        bool UpdateUserStatus(string userId, UserStatus status);
        bool UpdateLastUserTime(string userId);
        List<UserDto> SearchUserList(string text, string userId);


    }
}
