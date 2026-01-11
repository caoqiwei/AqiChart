

namespace AqiChart.Model.Shared
{
    public class FileAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;  // 缩略图URL(针对图片)
        public DateTime UploadTime { get; set; } = DateTime.UtcNow;
    }
}
