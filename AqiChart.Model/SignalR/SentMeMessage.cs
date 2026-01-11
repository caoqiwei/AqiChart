namespace AqiChart.Model.SignalR
{
    public class SentMeMessage
    {
        public string Id { get; set; }
        public string ReceiverId { get; set; }
        public DateTime SentAt { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
