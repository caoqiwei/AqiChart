
using SqlSugar;

namespace AqiChartServer.DB.Enties
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("friendships")]
    public partial class Friendships
    {
        public Friendships()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "user_id1")]
        public string UserId1 { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "user_id2")]
        public string UserId2 { get; set; }

        /// <summary>
        /// Desc:'Pending', 'Accepted', 'Rejected'
        ///     :“待定”、“接受”、“拒绝”
        /// Default:Pending
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Desc:添加时间
        /// Default:CURRENT_TIMESTAMP
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "created_at")]
        public DateTime? CreatedAt { get; set; }

    }
}
