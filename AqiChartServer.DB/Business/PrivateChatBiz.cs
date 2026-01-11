using AqiChart.Model.Dto;
using AqiChartServer.DB.Enties;
using AqiChartServer.DB.Enums;
using AqiChartServer.DB.Interface;
using System;

namespace AqiChartServer.DB.Business
{
    public class PrivateChatBiz: IPrivateChatBiz
    {
        /// <summary>
        /// 查询聊天记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PrivateChat> GetUserPrivateChats(string id)
        {
            return SqlSugarHelper.Db.Queryable<PrivateChat>().Where(x => x.SenderId == id || x.ReceiverId == id).ToList();
        }

        /// <summary>
        /// 添加聊天记录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public PrivateChat AddPrivateChats(PrivateChatDto dto)
        {
            PrivateChat model = new PrivateChat()
            {
                MessageId = dto.Id?? Guid.NewGuid().ToString(),
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content,
                IsRead = false,
                IsRecalled = false,
                ContentType = dto.ContentType,
                CreatedAt = DateTime.Now
            };

            return SqlSugarHelper.Db.Insertable(model).ExecuteCommand() > 0 ? model : null;

        }


        public List<PrivateChatDto> GetAllUnreadByUserId(string userId)
        {
            return SqlSugarHelper.Db.Queryable<PrivateChat>().Where(x => x.ReceiverId == userId && x.IsRead == false)
                .Select(x=> new PrivateChatDto()
                {
                    Content = x.Content,
                    ContentType = x.ContentType,
                    CreatedAt = x.CreatedAt,
                    Id = x.MessageId,
                    ReceiverId = x.ReceiverId,
                    SenderId = x.SenderId
                }).ToList();
        }

        public List<PrivateChatDto> GetUnreadUserChart(string userId,string friendId)
        {
            return SqlSugarHelper.Db.Queryable<PrivateChat>().Where(x => x.SenderId == friendId && x.ReceiverId == userId && x.IsRead == false)
                .Select(x => new PrivateChatDto()
                {
                    Content = x.Content,
                    ContentType = x.ContentType,
                    CreatedAt = x.CreatedAt,
                    Id = x.MessageId,
                    ReceiverId = x.ReceiverId,
                    SenderId = x.SenderId
                }).ToList();
        }

        public bool SetReadById(string id)
        {
            var chart = SqlSugarHelper.Db.Queryable<PrivateChat>().First(x => x.MessageId == id);
            chart.IsRead = true;
            var result = SqlSugarHelper.Db.Updateable(chart).ExecuteCommand() > 0;
            return result;

        }

        public bool SetReadByFriendId(string userId, string friendId)
        {
            var list = SqlSugarHelper.Db.Queryable<PrivateChat>().Where(x => x.SenderId == friendId && x.ReceiverId == userId && x.IsRead == false).ToList(); ;
            foreach (var item in list)
            {
                item.IsRead = true;
            }
            var result = SqlSugarHelper.Db.Updateable(list).ExecuteCommand() > 0;
            return result;
        }

        private PrivateChatDto ToDto(PrivateChat chat)
        {
            return new PrivateChatDto()
            {
                Content = chat.Content,
                ContentType = chat.ContentType,
                CreatedAt = chat.CreatedAt,
                Id = chat.MessageId,
                ReceiverId = chat.ReceiverId,
                SenderId = chat.SenderId
            };
        }

    }
}
