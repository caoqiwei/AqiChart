using AqiChart.Model.Enums;

namespace AqiChart.Model.Shared
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;  // 群聊ID，私聊时为空
        public MessageType Type { get; set; }
        public string Content { get; set; } = string.Empty;  // 文本内容或RTF
        public string AttachmentUrl { get; set; } = string.Empty;  // 附件URL
        public string AttachmentName { get; set; } = string.Empty; // 附件名称
        public long AttachmentSize { get; set; }  // 附件大小(字节)
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public MessageStatus Status { get; set; } = MessageStatus.Sending;
        public bool IsDeleted { get; set; }

        // 富文本相关
        public string RichTextData { get; set; } = string.Empty;  // RTF或HTML格式
        public string PreviewText { get; set; } = string.Empty;   // 预览文本
        public List<MentionInfo> Mentions { get; set; } = new List<MentionInfo>();
    }
}
