
namespace AqiChart.Model.Shared
{
    public class Emoji
    {
        public string Code { get; set; } = string.Empty;      // 如:😊
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;  // 自定义表情图片URL
        public bool IsUnicode { get; set; } = true;           // 是否是Unicode表情
    }
}
