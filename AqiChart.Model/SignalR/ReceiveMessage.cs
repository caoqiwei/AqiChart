namespace AqiChart.Model.SignalR
{
    public class ReceiveMessage
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string NickName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime SentAt { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
