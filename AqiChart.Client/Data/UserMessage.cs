using System;

namespace AqiChart.Client.Data
{
    public class UserSendMessage
    {
        public string UserId { get; set; }
        public string Nickname { get; set; }
        public string Message { get; set; }
    }

    public class UserReceiveMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 接收消息窗口ID
        /// </summary>
        public string ChartId { get; set; }
        /// <summary>
        /// 发送消息用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 发送人昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 发送人头像
        /// </summary>
        public string AvatarUrl { get; set; }
        /// <summary>
        /// 是否是本人发送的消息
        /// </summary>
        public bool IsMe { get; set; }
        /// <summary>
        /// 消息发送时间
        /// </summary>
        public DateTime Time { get; set; } = DateTime.Now;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }

        public string Type { get; set; }
    }
}
