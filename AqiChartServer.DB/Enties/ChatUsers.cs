using SqlSugar;

namespace AqiChartServer.DB.Enties
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("chat_users")]
    public partial class ChatUsers
    {
        public ChatUsers()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Desc:唯一用户名
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "username")]
        public string UserName { get; set; }

        /// <summary>
        /// Desc:BCrypt加密后的密码
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "password_hash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Desc:验证过的邮箱
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Desc:国际区号+号码（可选）
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Desc:头像CDN地址
        /// Default:/default-avatar.png
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "avatar_url")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Desc:用户显示名称（可重复）
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "nickname")]
        public string NickName { get; set; }

        /// <summary>
        /// Desc:实时状态 'online', 'offline', 'away'
        /// Default:offline
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Desc:注册时间
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Desc:最后更新时间
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Desc:最后在线时间
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "last_online")]
        public DateTime? LastOnline { get; set; }

    }
}
