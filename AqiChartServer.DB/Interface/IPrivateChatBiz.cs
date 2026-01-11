using AqiChart.Model.Dto;
using AqiChartServer.DB.Enties;

namespace AqiChartServer.DB.Interface
{
    public interface IPrivateChatBiz
    {
        List<PrivateChat> GetUserPrivateChats(string id);
        List<PrivateChatDto> GetAllUnreadByUserId(string userId);
        PrivateChat AddPrivateChats(PrivateChatDto dto);
        List<PrivateChatDto> GetUnreadUserChart(string userId, string friendId);
        bool SetReadById(string id);

        bool SetReadByFriendId(string userId, string friendId);
    }
}
