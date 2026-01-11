using System;

namespace AqiChart.Client.HttpClient
{
    /// <summary>
    /// API 客户端配置
    /// </summary>
    public class ApiClientConfig
    {
        public string BaseUrl { get; set; } = string.Empty; 
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
        public bool RetryOnFailure { get; set; } = false;
        public int MaxRetryCount { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public string? DefaultContentType { get; set; } = "application/json";
        public bool AutoRedirect { get; set; } = true;
        public int MaxRedirects { get; set; } = 10;
    }
}
