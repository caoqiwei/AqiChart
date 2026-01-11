using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AqiChart.Client.HttpClient
{
    /// <summary>
    /// ApiClient 扩展方法
    /// </summary>
    public static class ApiClientExtensions
    {
        /// <summary>
        /// 快速 GET 请求
        /// </summary>
        public static Task<ApiResponse<T>> GetAsync<T>(this ApiClient client, string endpoint,
            params (string Key, object Value)[] parameters)
        {
            var dict = parameters.ToDictionary(p => p.Key, p => p.Value);
            return client.GetAsync<T>(endpoint, dict);
        }

        /// <summary>
        /// 快速 POST 请求
        /// </summary>
        public static Task<ApiResponse<T>> PostAsync<T>(this ApiClient client, string endpoint,
            params (string Key, object Value)[] data)
        {
            var dict = data.ToDictionary(p => p.Key, p => p.Value);
            return client.PostAsync<T>(endpoint, dict);
        }

        /// <summary>
        /// 下载文件并保存
        /// </summary>
        public static async Task<bool> DownloadToFileAsync(this ApiClient client,
            string endpoint, string filePath, RequestConfig? config = null)
        {
            try
            {
                await client.DownloadFileAsync(endpoint, filePath, config);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        public static async Task<ApiResponse<T>> UploadFileAsync<T>(this ApiClient client,
            string endpoint, string filePath, string formFieldName = "file",
            Dictionary<string, string>? additionalData = null, RequestConfig? config = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using var content = new MultipartFormDataContent();

            // 添加文件
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, formFieldName, Path.GetFileName(filePath));

            // 添加额外数据
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    content.Add(new StringContent(kvp.Value), kvp.Key);
                }
            }

            return await client.PostMultipartAsync<T>(endpoint, content, config);
        }

        /// <summary>
        /// 设置超时
        /// </summary>
        public static ApiClient WithTimeout(this ApiClient client, TimeSpan timeout)
        {
            client.SetTimeout(timeout);
            return client;
        }

        /// <summary>
        /// 设置 Token
        /// </summary>
        public static ApiClient WithToken(this ApiClient client, string token)
        {
            client.SetBearerToken(token);
            return client;
        }

        /// <summary>
        /// 添加请求头
        /// </summary>
        public static ApiClient WithHeader(this ApiClient client, string name, string value)
        {
            client.AddDefaultHeader(name, value);
            return client;
        }

        /// <summary>
        /// 链式配置
        /// </summary>
        public static ApiClient Configure(this ApiClient client, Action<ApiClient> configureAction)
        {
            configureAction?.Invoke(client);
            return client;
        }

        /// <summary>
        /// 批量发送请求
        /// </summary>
        public static async Task<List<ApiResponse<T>>> SendBatchAsync<T>(
            this ApiClient client, IEnumerable<Func<Task<ApiResponse<T>>>> requests)
        {
            var tasks = requests.Select(request => request());
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}
