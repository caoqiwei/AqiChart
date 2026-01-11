using System;

namespace AqiChart.Client.HttpClient
{

    /// <summary>
    /// HTTP 方法枚举
    /// </summary>
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }

    /// <summary>
    /// 请求配置
    /// </summary>
    public class RequestConfig
    {
        public Dictionary<string, string>? Headers { get; set; }
        public TimeSpan? Timeout { get; set; }
        public string? ContentType { get; set; }
        public bool ThrowOnError { get; set; } = false;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
    }
}
