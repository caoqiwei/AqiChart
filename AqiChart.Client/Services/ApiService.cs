using AqiChart.Client.Common;
using AqiChart.Client.Data;
using AqiChart.Client.HttpClient;
using AqiChart.Model.Dto;
using AqiChart.Model.Shared;
using UserDto = AqiChart.Client.Data.UserDto;

namespace AqiChart.Client.Services
{
    public class ApiService
    {

        public static async Task<UserModel> UserLogin(LoginModel loginModel)
        {
           var res =  await ApiClient.Instance.PostAsync<UserModel>("/api/Auth/Login",
                new { userName = loginModel.Account, password = loginModel.Password });
            //Response<UserModel> res = await HttpHelper.PostAsync<Response<UserModel>>("/api/Auth/Login",
            //    new { userName = loginModel.Account, password = loginModel.Password });
            
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        public static async Task<UserModel> GetUserInfo()
        {
            var res = await ApiClient.Instance.PostAsync<UserModel>("/api/Auth/UserInfo");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        public static async Task<List<UserDto>> GetFriends()
        {
            ApiResponse<List<UserDto>> res = await ApiClient.Instance.PostAsync<List<UserDto>>("/api/Friend/GetFriends");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        public static async Task<bool> UpdateLastUserTime()
        {
            ApiResponse<bool> res = await ApiClient.Instance.PostAsync<bool>("/api/User/UpdateLastUserTime");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        public static async Task<List<UserDto>> SearchUserList(SearchDto dto)
        {
            ApiResponse<List<UserDto>> res = await ApiClient.Instance.PostAsync<List<UserDto>>("/api/User/SearchUserList", dto);
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> AddFriend(string friendId)
        {

            ApiResponse<bool> res = await ApiClient.Instance.PostUrlAsync<bool>("/api/Friend/AddFriend",
             new Dictionary<string, object>
            {
                { "friendId", friendId }
            });
            //new FriendDto() { UserId2 = friendId });
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 同意好友申请
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> ThroughFriend(string friendId)
        {
            ApiResponse<bool> res = await ApiClient.Instance.PostUrlAsync<bool>("/api/Friend/ThroughFriend",
             new Dictionary<string, object>
            {
                { "friendId", friendId }
            });
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 拒绝好友申请
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> RejectFriend(string friendId)
        {
            ApiResponse<bool> res = await ApiClient.Instance.PostUrlAsync<bool>("/api/Friend/RejectFriend",
            new Dictionary<string, object>
            {
                { "friendId", friendId }
            });
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 获取被拒列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<UserDto>> GetRejectFriends()
        {
            ApiResponse<List<UserDto>> res = await ApiClient.Instance.PostAsync<List<UserDto>>("/api/Friend/GetRejectFriends");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 获取申请列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<UserDto>> GetApplyFriends()
        {
            ApiResponse<List<UserDto>> res = await ApiClient.Instance.PostAsync<List<UserDto>>("/api/Friend/GetApplyFriends");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        #region 聊天记录
        /// <summary>
        /// 获取全部未读信息
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<PrivateChatDto>> GetAllUnreadChart()
        {
            ApiResponse<List<PrivateChatDto>> res = await ApiClient.Instance.PostAsync<List<PrivateChatDto>>("/api/PrivateChat/GetAllUnreadChart");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 获取好友未读信息
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<PrivateChatDto>> GetUnreadChart()
        {
            ApiResponse<List<PrivateChatDto>> res = await ApiClient.Instance.PostAsync<List<PrivateChatDto>>("/api/PrivateChat/GetUnreadChart");
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 单条记录已读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> SetReadById(string id)
        {
            ApiResponse<bool> res = await ApiClient.Instance.PostUrlAsync<bool>("/api/PrivateChat/SetReadById",
            new Dictionary<string, object>
            {
                { "id", id }
            });
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        /// <summary>
        /// 指定好友信息已读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> SetReadByFriendId(string friendId)
        {
            ApiResponse<bool> res = await ApiClient.Instance.PostUrlAsync<bool>("/api/PrivateChat/SetReadByFriendId",
            new Dictionary<string, object>
            {
                { "friendId", friendId }
            });
            if (res.Code != 200)
            {
                throw new Exception(res.Msg);
            }
            return res.Data;
        }

        #endregion


    }
}
