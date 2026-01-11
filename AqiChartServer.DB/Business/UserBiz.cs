using AqiChartServer.DB.Enties;
using AqiChart.Model.Dto;
using AqiChartServer.DB.Interface;

namespace AqiChartServer.DB.Business
{
    public class UserBiz: IUserBiz
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public ChatUsers GetUsers(string username)
        {
            return SqlSugarHelper.Db.Queryable<ChatUsers>().First(x => x.UserName == username);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public ChatUsers GetUsers(string username,string pwd) { 
            return SqlSugarHelper.Db.Queryable<ChatUsers>().First(x=>x.UserName==username&&x.PasswordHash==pwd); 
        }
        /// <summary>
        /// 根据Id查询用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ChatUsers GetUserInfo(string userId)
        {
            return SqlSugarHelper.Db.Queryable<ChatUsers>().First(x => x.UserId == userId);
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <returns></returns>
        public List<UserDto> GetUserList()
        {
            return SqlSugarHelper.Db.Queryable<ChatUsers>().OrderByDescending(x=>x.CreatedAt).Select(x=> new UserDto() {
                Id = x.UserId,
                AvatarUrl = x.AvatarUrl,
                Email = x.Email,
                NickName = x.NickName,
                Phone = x.Phone,
                UserName = x.UserName,
                Status = x.Status
            }).ToList();
        }

        /// <summary>
        /// 更新用户状态
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        public bool UpdateUserStatus(string userId, UserStatus status)
        {
            var user = SqlSugarHelper.Db.Queryable<ChatUsers>().First(x => x.UserId == userId);
            user.Status = status.ToString();
            if (status == UserStatus.online)
            {
                user.LastOnline = DateTime.Now;
            }
            return SqlSugarHelper.Db.Updateable<ChatUsers>(user).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 更新用户最后在线时间
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public bool UpdateLastUserTime(string userId)
        {
            var user = SqlSugarHelper.Db.Queryable<ChatUsers>().First(x => x.UserId == userId);
            user.LastOnline = DateTime.Now;
            return SqlSugarHelper.Db.Updateable<ChatUsers>(user).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 查询用户列表(过滤掉自己)
        /// </summary>
        /// <returns></returns>
        public List<UserDto> SearchUserList(string text, string userId) 
        {
            text = text.Trim();
            return SqlSugarHelper.Db.Queryable<ChatUsers>()
                 .LeftJoin<Friendships>((u, f) => u.UserId == f.UserId2 && f.UserId1 == userId)
                 .Where((u,f) => u.UserId != userId&&u.NickName.Contains(text) && f.Id == null)
                 .OrderByDescending(u => u.CreatedAt).Select(u => new UserDto()
                 {
                     Id = u.UserId,
                     AvatarUrl = u.AvatarUrl,
                     Email = u.Email,
                     NickName = u.NickName,
                     Phone = u.Phone,
                     UserName = u.UserName,
                     Status = u.Status
                 }).Take(20).ToList();
        }

    }
}
