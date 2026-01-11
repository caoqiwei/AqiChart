using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace AqiChartServer.DB.Enties
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("private_chat")]
    public partial class PrivateChat
    {
        public PrivateChat()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, ColumnName = "message_id", IsIdentity = false)]
        public string MessageId { get; set; }

        /// <summary>
        /// Desc:发送方UUID
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "sender_id")]
        public string SenderId { get; set; }

        /// <summary>
        /// Desc:接收方UUID
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "receiver_id")]
        public string ReceiverId { get; set; }

        /// <summary>
        /// Desc:加密消息体（JSON格式）
        /// Default:
        /// Nullable:False
        /// </summary>
        [SugarColumn(ColumnName = "content")]
        public string Content { get; set; }

        /// <summary>
        /// Desc:
        /// Default:text
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "content_type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Desc:文件元数据 {"name":"file.pdf","size":5242880,"hash":"sha256:..."}
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "file_metadata", IsJson = true)]
        public object FileMetadata { get; set; }

        /// <summary>
        /// Desc:0=未读，1=已读
        /// Default:true
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "is_read")]
        public bool? IsRead { get; set; }

        /// <summary>
        /// Desc:0=正常，1=撤回
        /// Default:true
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "is_recalled")]
        public bool? IsRecalled { get; set; }

        /// <summary>
        /// Desc:
        /// Default:CURRENT_TIMESTAMP(3)
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Desc:
        /// Default:CURRENT_TIMESTAMP(3)
        /// Nullable:True
        /// </summary>
        [SugarColumn(ColumnName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }

    }
}
