
namespace AqiChart.Model.Dto
{
    public class PrivateChatDto
    {
        public string Id { get; set; }

        public string ContentType { get; set; }
        public string SenderId { get; set; }

        public string ReceiverId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        
        //public object FileMetadata { get; set; }
    }

    public enum ContentType
    {
        text,
        image,
        file
    }

}
