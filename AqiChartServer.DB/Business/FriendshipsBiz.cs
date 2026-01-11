using AqiChartServer.DB.Enties;
using AqiChart.Model.Dto;
using AqiChartServer.DB.Interface;
using AqiChartServer.DB.Enums;


namespace AqiChartServer.DB.Business
{
    public class FriendshipsBiz: IFriendshipsBiz
    {

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserDto> GetFriends(string userId)
        {
            var query5 = SqlSugarHelper.Db.Queryable<Friendships>()
             .InnerJoin<ChatUsers>((o, cus) => o.UserId2 == cus.UserId)
             .Where(o => o.UserId1 == userId && o.Status == FriendshipsStatusEnum.Accepted.ToString())
             .Select((o, cus) => new UserDto { Id = cus.UserId,  AvatarUrl = cus.AvatarUrl, Email = cus.Email, 
                 NickName = cus.NickName, Phone = cus.Phone, UserName = cus.UserName, Status = cus.Status })
             .ToList();
            return query5;
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="userId">发起用户</param>
        /// <param name="friendId">要添加的好友</param>
        /// <returns></returns>
        public bool AddFriend(string userId, string friendId)
        {
            var list = SqlSugarHelper.Db.Queryable<Friendships>().Where(x => (x.UserId1 == userId && x.UserId2 == friendId) || (x.UserId1 == friendId && x.UserId2 == userId)).ToList();
            if (list.Count > 0)
            {
                var deleteCount = SqlSugarHelper.Db.Deleteable(list).ExecuteCommand();
                if (deleteCount == 0) throw new Exception("添加失败");
            }
            List<Friendships> friendships = new List<Friendships>();
            friendships.Add(new Friendships()
            {
                UserId1 = userId,
                UserId2 = friendId,
                Status = FriendshipsStatusEnum.Apply.ToString(),
                CreatedAt = DateTime.Now,
            });
            friendships.Add(new Friendships()
            {
                UserId1 = friendId,
                UserId2 = userId,
                CreatedAt = DateTime.Now,
                Status = FriendshipsStatusEnum.Pending.ToString()
            });
            var result = SqlSugarHelper.Db.Insertable(friendships).ExecuteCommand() > 0;
            return result;
        }

        /// <summary>
        /// 同意并通过添加好友
        /// </summary>
        /// <param name="userId">同意用户</param>
        /// <param name="friendId">发起好友用户</param>
        /// <returns></returns>
        public bool ThroughFriend(string userId, string friendId)
        {
            List<Friendships> friendships = SqlSugarHelper.Db.Queryable<Friendships>().Where(x => (x.UserId1 == userId && x.UserId2 == friendId) || (x.UserId1 == friendId && x.UserId2 == userId)).ToList();
            foreach (var item in friendships)
            {
                item.Status = FriendshipsStatusEnum.Accepted.ToString();
            }
            var result = SqlSugarHelper.Db.Updateable(friendships).ExecuteCommand() > 0;
            return result;
        }

        /// <summary>
        /// 拒绝添加好友
        /// </summary>
        /// <param name="userId">拒绝人</param>
        /// <param name="friendId">被拒绝的用户</param>
        /// <returns></returns>
        public bool RejectFriend(string userId, string friendId)
        {
            Friendships friendship = SqlSugarHelper.Db.Queryable<Friendships>().First(x => x.UserId1 == userId && x.UserId2 == friendId);
            friendship.Status = FriendshipsStatusEnum.Rejected.ToString();
            var result = SqlSugarHelper.Db.Updateable(friendship).ExecuteCommand() > 0;
            return result;
        }


        /// <summary>
        /// 查询好友列表
        /// </summary>
        /// <param name="userId">用户</param>
        /// <param name="status">好友状态</param>
        /// <returns></returns>
        public List<UserDto> GetFriendList(string userId, FriendshipsStatusEnum status)
        {
            return SqlSugarHelper.Db.Queryable<ChatUsers>()
                 .InnerJoin<Friendships>((u, f) => u.UserId == f.UserId2)
                 .Where((u, f) => f.Status == status.ToString() && f.UserId1 == userId)
                 .OrderByDescending(u => u.CreatedAt).Select(u => new UserDto()
                 {
                     Id = u.UserId,
                     AvatarUrl = u.AvatarUrl,
                     Email = u.Email,
                     NickName = u.NickName,
                     Phone = u.Phone,
                     UserName = u.UserName,
                     Status = u.Status
                 }).ToList();
        }

    }
}
