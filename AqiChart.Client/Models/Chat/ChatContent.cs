

namespace AqiChart.Client.Models.Chat
{
    public class ChatContent
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string NickName { get; set; }

        public string AvatarUrl { get; set; }
        public string Type { get; set; }
        public bool IsMe { get; set; }

        public string Content { get; set; }

        public DateTime DateTime { get; set; }

    }
}
