using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Web;

namespace AqiChart.Client.Common
{
    /// <summary>
    /// HTTP 请求辅助类 (静态方法)
    /// </summary>
    public class HttpHelper
    {
        private static readonly System.Net.Http.HttpClient _httpClient;

        /// <summary>
        /// 静态构造函数，初始化 HttpClient
        /// </summary>
        static HttpHelper()
        {
            _httpClient = new System.Net.Http.HttpClient();

            // 配置token
            string accessToken = SettingConfig.Token; 
            //if (!string.IsNullOrEmpty(accessToken))
            //    _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            if (!string.IsNullOrEmpty(accessToken))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            // 配置默认请求头
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Net8HttpHelper/1.0");

            // 设置默认超时时间
            _httpClient.Timeout = TimeSpan.FromSeconds(60);

            // 设置默认超时时间
            _httpClient.BaseAddress = new Uri(SettingConfig.ApiBaseUrl);
        }

        #region 配置属性

        ///// <summary>
        ///// 获取或设置基础地址
        ///// </summary>
        //public static string BaseAddress
        //{
        //    get => _httpClient.BaseAddress?.ToString();
        //    set => _httpClient.BaseAddress = value != null ? new Uri(value) : null;
        //}

        /// <summary>
        /// 获取或设置超时时间（秒）
        /// </summary>
        public static int Timeout
        {
            get => (int)_httpClient.Timeout.TotalSeconds;
            set => _httpClient.Timeout = TimeSpan.FromSeconds(value);
        }


        #endregion

        #region GET 请求

        /// <summary>
        /// 发送 GET 请求（异步）
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="param">查询参数</param>
        /// <returns>响应内容字符串</returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> param = null)
        {
            string fullUrl = BuildUrlWithParams(url, param);

            using (var request = new HttpRequestMessage(HttpMethod.Get, fullUrl))
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送 GET 请求（同步）
        /// </summary>
        public static string Get(string url, Dictionary<string, string> param = null)
        {
            return GetAsync(url, param).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 GET 请求并返回指定类型（异步）
        /// </summary>
        public static async Task<T> GetAsync<T>(string url, Dictionary<string, string> param = null)
        {
            var json = await GetAsync(url, param);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// 发送 GET 请求并返回指定类型（同步）
        /// </summary>
        public static T Get<T>(string url, Dictionary<string, string> param = null)
        {
            return GetAsync<T>(url, param).GetAwaiter().GetResult();
        }

        #endregion

        #region POST 请求

        /// <summary>
        /// 发送 POST 请求（异步）
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="data">请求数据</param>
        /// <returns>响应内容字符串</returns>
        public static async Task<string> PostAsync(string url, object data = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                if (data != null)
                {
                    //if (_httpClient.DefaultRequestHeaders.Contains("Accept"))
                    //{
                    //    _httpClient.DefaultRequestHeaders.Remove("Accept");
                    //}
                    //_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var jsonContent = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }
                
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        

        //var content = new FormUrlEncodedContent(formData);

        /// <summary>
        /// 发送 POST 请求（同步）
        /// </summary>
        public static string Post(string url, object data = null)
        {
            return PostAsync(url, data).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 POST 请求并返回指定类型（异步）
        /// </summary>
        public static async Task<T> PostAsync<T>(string url, object data = null)
        {
            string json = await PostAsync(url, data);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public static async Task<string> PostFormAsync(string url, Dictionary<string, string> data = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {

                if (data != null)
                {
                    request.Content = new FormUrlEncodedContent(data);
                }
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送 POST 请求（同步）
        /// </summary>
        public static string PostForm(string url, Dictionary<string, string> data = null)
        {
            return PostFormAsync(url, data).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 POST 请求并返回指定类型（异步）
        /// </summary>
        public static async Task<T> PostFormAsync<T>(string url, Dictionary<string, string> data = null)
        {
            string json = await PostFormAsync(url, data);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// 发送 POST 请求并返回指定类型（同步）
        /// </summary>
        public static T Post<T>(string url, object data = null)
        {
            return PostAsync<T>(url, data).GetAwaiter().GetResult();
        }

        #endregion

        #region PUT 请求

        /// <summary>
        /// 发送 PUT 请求（异步）
        /// </summary>
        public static async Task<string> PutAsync(string url, object data = null)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, url))
            {
                if (data != null)
                {
                    var jsonContent = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送 PUT 请求（同步）
        /// </summary>
        public static string Put(string url, object data = null)
        {
            return PutAsync(url, data).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 PUT 请求并返回指定类型（异步）
        /// </summary>
        public static async Task<T> PutAsync<T>(string url, object data = null)
        {
            var json = await PutAsync(url, data);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// 发送 PUT 请求并返回指定类型（同步）
        /// </summary>
        public static T Put<T>(string url, object data = null)
        {
            return PutAsync<T>(url, data).GetAwaiter().GetResult();
        }

        #endregion

        #region DELETE 请求

        /// <summary>
        /// 发送 DELETE 请求（异步）
        /// </summary>
        public static async Task<string> DeleteAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 发送 DELETE 请求（同步）
        /// </summary>
        public static string Delete(string url)
        {
            return DeleteAsync(url).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送 DELETE 请求并返回指定类型（异步）
        /// </summary>
        public static async Task<T> DeleteAsync<T>(string url)
        {
            var json = await DeleteAsync(url);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// 发送 DELETE 请求并返回指定类型（同步）
        /// </summary>
        public static T Delete<T>(string url)
        {
            return DeleteAsync<T>(url).GetAwaiter().GetResult();
        }

        #endregion


        #region 私有辅助方法

        /// <summary>
        /// 添加默认请求头到请求
        /// </summary>
        public static void SetTokenToHeader()
        {
            string accessToken = SettingConfig.Token;
            if (!string.IsNullOrEmpty(accessToken)&& !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken.ToString()));
            //!_httpClient.Headers.Contains(header.Key)
        }

        /// <summary>
        /// 构建带参数的 URL
        /// </summary>
        private static string BuildUrlWithParams(string url, Dictionary<string, string> param)
        {
            if (param == null || !param.Any())
                return url;

            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (var item in param)
            {
                query[item.Key] = item.Value;
            }

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        #endregion

        #region MyRegion

        //// 示例 1: GET 请求带参数
        //var queryParams = new Dictionary<string, string>
        //        {
        //            { "page", "1" },
        //            { "pageSize", "20" },
        //            { "sort", "name" },
        //            { "active", "true" }
        //        };

        //string usersJson = await HttpHelper.GetAsync("/api/users", queryParams);
        //Console.WriteLine("GET Response: " + usersJson);

        //        // 示例 2: GET 请求带参数并直接反序列化
        //        var users = await HttpHelper.GetAsync<ApiResponse<List<User>>>("/api/users", queryParams);
        //Console.WriteLine($"获取到 {users.Data.Count} 个用户");

        //        // 示例 3: GET 请求不带参数
        //        var user = await HttpHelper.GetAsync<ApiResponse<User>>("/api/users/1");
        //Console.WriteLine($"用户信息: {user.Data.Name}");

        //        // 示例 4: POST 请求
        //        var newUser = new { Name = "John Doe", Email = "john@example.com" };
        //string result = await HttpHelper.PostAsync("/api/users", newUser);
        //Console.WriteLine("POST Response: " + result);

        //        // 示例 5: POST 请求并直接反序列化
        //        var createdUser = await HttpHelper.PostAsync<ApiResponse<User>>("/api/users", newUser);
        //Console.WriteLine($"创建用户成功: {createdUser.Data.Name}");

        //        // 示例 6: PUT 请求
        //        var updateData = new { Name = "Jane Doe" };
        //await HttpHelper.PutAsync("/api/users/1", updateData);

        //// 示例 7: DELETE 请求
        //await HttpHelper.DeleteAsync("/api/users/1");

        //// 示例 8: 动态更新配置
        //HttpHelper.SetBearerToken("new-access-token");
        //        HttpHelper.SetDefaultHeader("X-Client-Version", "1.1.0");
                
        //        // 使用新配置发送请求
        //        var updatedUsers = await HttpHelper.GetAsync<List<User>>("/api/users",
        //            new Dictionary<string, string> { { "updatedSince", "2024-01-01" } });

        //// 示例 9: 同步方法调用
        //string syncResult = HttpHelper.Get("/api/users",
        //    new Dictionary<string, string> { { "sync", "true" } });
        //Console.WriteLine("同步GET结果: " + syncResult);

        #endregion

    }
}
