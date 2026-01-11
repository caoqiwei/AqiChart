using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace AqiChart.Client.HttpClient
{
    /// <summary>
    /// 通用 HTTP 客户端接口
    /// </summary>
    public interface IApiClient
    {
        // 配置管理
        void Configure(ApiClientConfig config);
        void SetBaseUrl(string baseUrl);
        void SetTimeout(TimeSpan timeout);
        void SetBearerToken(string token);
        void ClearBearerToken();
        void AddDefaultHeader(string name, string value);
        void RemoveDefaultHeader(string name);

        // 基本请求方法
        Task<ApiResponse<T>> GetAsync<T>(string endpoint, object? parameters = null, RequestConfig? config = null);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, RequestConfig? config = null);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null, RequestConfig? config = null);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, object? parameters = null, RequestConfig? config = null);
        Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null, RequestConfig? config = null);

        // 高级请求方法
        Task<ApiResponse<T>> PostFormAsync<T>(string endpoint, Dictionary<string, string> formData, RequestConfig? config = null);
        Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content, RequestConfig? config = null);
        Task<ApiResponse<T>> SendJsonAsync<T>(string endpoint, HttpMethod method, object? data = null, RequestConfig? config = null);

        // 文件下载
        Task<ApiResponse<byte[]>> DownloadFileAsync(string endpoint, RequestConfig? config = null);
        Task DownloadFileAsync(string endpoint, string filePath, RequestConfig? config = null);

        // 流式处理
        Task<ApiResponse<Stream>> GetStreamAsync(string endpoint, object? parameters = null, RequestConfig? config = null);
    }
}
