

namespace AqiChart.Model.Shared
{
    public class EmojiPackage
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<Emoji> Emojis { get; set; } = new List<Emoji>();
        public bool IsSystem { get; set; }
        public int Order { get; set; }
    }
}
