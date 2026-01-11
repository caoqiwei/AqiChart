using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace AqiChart.Client.HttpClient
{
    /// <summary>
    /// 通用 HTTP 客户端（单例模式）
    /// </summary>
    public sealed class ApiClient : IApiClient, IDisposable
    {
        #region 单例实现
        private static readonly Lazy<ApiClient> _instance = new Lazy<ApiClient>(() => new ApiClient());

        public static ApiClient Instance => _instance.Value;

        private static readonly Lazy<Dictionary<string, ApiClient>> _namedInstances =
            new Lazy<Dictionary<string, ApiClient>>(() => new Dictionary<string, ApiClient>());

        public static ApiClient GetNamedInstance(string name)
        {
            if (!_namedInstances.Value.ContainsKey(name))
            {
                lock (_namedInstances.Value)
                {
                    if (!_namedInstances.Value.ContainsKey(name))
                    {
                        _namedInstances.Value[name] = new ApiClient();
                    }
                }
            }
            return _namedInstances.Value[name];
        }
        #endregion

        #region 私有字段
        private readonly System.Net.Http.HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly JsonSerializerOptions _jsonOptions;
        private ApiClientConfig _config;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
        #endregion

        #region 构造函数
        private ApiClient()
        {
            _config = new ApiClientConfig();
            _httpClientHandler = CreateHttpClientHandler();
            _httpClient = new System.Net.Http.HttpClient(_httpClientHandler)
            {
                Timeout = _config.Timeout
            };


            ConfigureHttpClient();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                //WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                
                // 添加日期时间转换器
                Converters = {
                    new JsonDateTimeConverter()  // 自定义转换器
                },
                // 设置日期处理方式
                WriteIndented = true
            };
        }
        #endregion

        #region 配置管理
        public void Configure(ApiClientConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _semaphore.Wait();
            try
            {
                _config = config;
                ConfigureHttpClient();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private HttpClientHandler CreateHttpClientHandler()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = _config.AutoRedirect,
                MaxAutomaticRedirections = _config.MaxRedirects,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
        }

        private void ConfigureHttpClient()
        {
            if (!string.IsNullOrWhiteSpace(_config.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            }

            _httpClient.Timeout = _config.Timeout;

            // 配置 HttpClientHandler
            _httpClientHandler.AllowAutoRedirect = _config.AutoRedirect;
            _httpClientHandler.MaxAutomaticRedirections = _config.MaxRedirects;

            // 清空并重新设置默认请求头
            _httpClient.DefaultRequestHeaders.Clear();

            // 设置 Accept 头
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // 设置 User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WpfApp-ApiClient/1.0");

            // 重新添加默认请求头
            foreach (var header in _defaultHeaders)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        public void SetBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

            _semaphore.Wait();
            try
            {
                _config.BaseUrl = baseUrl.TrimEnd('/') + "/";
                _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _semaphore.Wait();
            try
            {
                _config.Timeout = timeout;
                _httpClient.Timeout = timeout;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void SetBearerToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ClearBearerToken();
                return;
            }

            _semaphore.Wait();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //_httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ClearBearerToken()
        {
            _semaphore.Wait();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void AddDefaultHeader(string name, string value)
        {
            _semaphore.Wait();
            try
            {
                if (_defaultHeaders.ContainsKey(name))
                    _defaultHeaders[name] = value;
                else
                    _defaultHeaders.Add(name, value);

                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void RemoveDefaultHeader(string name)
        {
            _semaphore.Wait();
            try
            {
                if (_defaultHeaders.ContainsKey(name))
                {
                    _defaultHeaders.Remove(name);
                    _httpClient.DefaultRequestHeaders.Remove(name);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region 基本请求方法
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, object? parameters = null, RequestConfig? config = null)
        {
            return await SendJsonAsync<T>(endpoint, HttpMethod.GET, parameters, config);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, RequestConfig? config = null)
        {
            return await SendJsonAsync<T>(endpoint, HttpMethod.POST, data, config);
        }

        public async Task<ApiResponse<T>> PostUrlAsync<T>(string endpoint, Dictionary<string, object> formData, RequestConfig? config = null)
        {
            if(formData!=null && formData.Count > 0)
            {
                endpoint = BuildDictionaryUrlWithQueryString(endpoint, formData);
            }
            
            return await SendUrlAsync<T>(endpoint, HttpMethod.POST, config);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null, RequestConfig? config = null)
        {
            return await SendJsonAsync<T>(endpoint, HttpMethod.PUT, data, config);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, object? parameters = null, RequestConfig? config = null)
        {
            return await SendJsonAsync<T>(endpoint, HttpMethod.DELETE, parameters, config);
        }

        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null, RequestConfig? config = null)
        {
            return await SendJsonAsync<T>(endpoint, HttpMethod.PATCH, data, config);
        }
        #endregion

        #region 高级请求方法
        public async Task<ApiResponse<T>> PostFormAsync<T>(string endpoint, Dictionary<string, string> formData, RequestConfig? config = null)
        {
            var content = new FormUrlEncodedContent(formData);
            return await SendRawAsync<T>(endpoint, HttpMethod.POST, content,
                new RequestConfig
                {
                    ContentType = "application/x-www-form-urlencoded",
                    Headers = config?.Headers,
                    Timeout = config?.Timeout,
                    ThrowOnError = config?.ThrowOnError ?? false,
                    CancellationToken = config?.CancellationToken ?? CancellationToken.None
                });
        }


        public async Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content, RequestConfig? config = null)
        {
            return await SendRawAsync<T>(endpoint, HttpMethod.POST, content, config);
        }

        public async Task<ApiResponse<T>> SendJsonAsync<T>(string endpoint, HttpMethod method, object? data = null, RequestConfig? config = null)
        {
            HttpContent? content = null;
            if (data != null && method != HttpMethod.GET && method != HttpMethod.DELETE)
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await SendRawAsync<T>(endpoint, method, content, config);
        }

        private async Task<ApiResponse<T>> SendRawAsync<T>(string endpoint, HttpMethod method, HttpContent? content = null, RequestConfig? config = null)
        {
            var response = new ApiResponse<T>();
            var requestTime = DateTime.UtcNow;

            try
            {
                var request = CreateHttpRequestMessage(endpoint, method, content, config);
                var cancellationToken = config?.CancellationToken ?? CancellationToken.None;

                HttpResponseMessage httpResponse;
                if (_config.RetryOnFailure)
                {
                    httpResponse = await ExecuteWithRetryAsync(request, cancellationToken);
                }
                else
                {
                    httpResponse = await _httpClient.SendAsync(request, cancellationToken);
                }

                response = await ProcessResponseAsync<T>(httpResponse, requestTime);

                if (!response.IsSuccess && config?.ThrowOnError == true)
                {
                    throw new HttpRequestException($"Request failed: {response.Msg}");
                }
            }
            catch (TaskCanceledException)
            {
                response.IsSuccess = false;
                response.Msg = "Request timeout";
                response.Code = 408;
                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Msg = ex.Message;
                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }

            return response;
        }

        private async Task<ApiResponse<T>> SendUrlAsync<T>(string url, HttpMethod method, RequestConfig? config = null)
        {
            var response = new ApiResponse<T>();
            var requestTime = DateTime.UtcNow;

            try
            {
                
                var cancellationToken = config?.CancellationToken ?? CancellationToken.None;

                HttpResponseMessage httpResponse;
                if (_config.RetryOnFailure)
                {
                    httpResponse = await ExecuteWithRetryAsync(url, cancellationToken);
                }
                else
                {
                    httpResponse = await _httpClient.PostAsync(url, null, cancellationToken);
                }

                response = await ProcessResponseAsync<T>(httpResponse, requestTime);

                if (!response.IsSuccess && config?.ThrowOnError == true)
                {
                    throw new HttpRequestException($"Request failed: {response.Msg}");
                }
            }
            catch (TaskCanceledException)
            {
                response.IsSuccess = false;
                response.Msg = "Request timeout";
                response.Code = 408;
                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Msg = ex.Message;
                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }

            return response;
        }
        #endregion

        #region 文件下载和流处理
        public async Task<ApiResponse<byte[]>> DownloadFileAsync(string endpoint, RequestConfig? config = null)
        {
            return await GetAsync<byte[]>(endpoint, null, config);
        }

        public async Task DownloadFileAsync(string endpoint, string filePath, RequestConfig? config = null)
        {
            var response = await GetStreamAsync(endpoint, null, config);

            if (response.IsSuccess && response.Data != null)
            {
                using var fileStream = File.Create(filePath);
                await response.Data.CopyToAsync(fileStream);
                response.Data.Close();
            }
            else
            {
                throw new HttpRequestException($"Download failed: {response.Msg}");
            }
        }

        public async Task<ApiResponse<Stream>> GetStreamAsync(string endpoint, object? parameters = null, RequestConfig? config = null)
        {
            var response = new ApiResponse<Stream>();
            var requestTime = DateTime.UtcNow;

            try
            {
                var request = CreateHttpRequestMessage(endpoint, HttpMethod.GET, null, config);
                if (parameters != null)
                {
                    request.RequestUri = new Uri(BuildUrlWithQueryString(endpoint, parameters));
                }

                var httpResponse = await _httpClient.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    config?.CancellationToken ?? CancellationToken.None);

                if (httpResponse.IsSuccessStatusCode)
                {
                    response.IsSuccess = true;
                    response.Data = await httpResponse.Content.ReadAsStreamAsync();
                    response.Code = (int)httpResponse.StatusCode;
                    response.Headers = ConvertHeaders(httpResponse.Headers);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Msg = $"HTTP Error: {httpResponse.StatusCode}";
                    response.Code = (int)httpResponse.StatusCode;
                }

                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Msg = ex.Message;
                response.RequestTime = requestTime;
                response.ResponseTime = DateTime.UtcNow - requestTime;
            }

            return response;
        }
        #endregion

        #region 私有辅助方法
        private HttpRequestMessage CreateHttpRequestMessage(string endpoint, HttpMethod method,
            HttpContent? content, RequestConfig? config)
        {
            var request = new HttpRequestMessage
            {
                Method = new System.Net.Http.HttpMethod(method.ToString()),
                Content = content
            };

            // 构建请求 URL
            var url = endpoint;
            if ((method == HttpMethod.GET || method == HttpMethod.DELETE) &&
                content is StringContent stringContent)
            {
                try
                {
                    var json = stringContent.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(json))
                    {
                        var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
                        if (parameters != null)
                        {
                            url = BuildUrlWithQueryString(endpoint, parameters);
                        }
                    }
                }
                catch
                {
                    // 如果解析失败，使用原始 URL
                }
            }

            // 设置请求 URI
            if (string.IsNullOrWhiteSpace(_config.BaseUrl))
            {
                request.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);
            }
            else
            {
                request.RequestUri = new Uri(new Uri(_config.BaseUrl), url);
            }

            // 添加自定义请求头
            if (config?.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // 设置超时（如果指定）
            if (config?.Timeout.HasValue == true)
            {
                var cts = new CancellationTokenSource(config.Timeout.Value);
                if (config.CancellationToken != CancellationToken.None)
                {
                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                        config.CancellationToken, cts.Token);
                }
            }

            return request;
        }

        private string BuildDictionaryUrlWithQueryString(string url, Dictionary<string, object> parameters)
        {
            if (parameters == null) return url;

            var queryString = BuildDictionaryQueryString(parameters);
            return string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
        }

        private string BuildUrlWithQueryString(string url, object parameters)
        {
            if (parameters == null) return url;

            var queryString = BuildQueryString(parameters);
            return string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
        }

        private string BuildQueryString(object parameters)
        {
            var queryParams = new List<string>();

            if (parameters is Dictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Value != null)
                    {
                        queryParams.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString()!)}");
                    }
                }
            }
            else
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(parameters);
                    if (value != null)
                    {
                        queryParams.Add($"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(value.ToString()!)}");
                    }
                }
            }

            return string.Join("&", queryParams);
        }

        private string BuildDictionaryQueryString(Dictionary<string, object> parameters)
        {
            var queryParams = new List<string>();

            foreach (var kvp in parameters)
            {
                if (kvp.Value != null)
                {
                    queryParams.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString()!)}");
                }
            }

            return string.Join("&", queryParams);
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int retryCount = 0; retryCount <= _config.MaxRetryCount; retryCount++)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        await Task.Delay(_config.RetryDelay, cancellationToken);
                    }

                    response = await _httpClient.PostAsync(url,null, cancellationToken);

                    // 如果是成功的响应或者客户端错误（4xx），直接返回
                    if (response.IsSuccessStatusCode ||
                        (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                    {
                        return response;
                    }
                }
                catch (Exception ex) when (retryCount < _config.MaxRetryCount)
                {
                    lastException = ex;
                    continue;
                }
            }

            return response ?? throw lastException ?? new HttpRequestException("Request failed after all retries");
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int retryCount = 0; retryCount <= _config.MaxRetryCount; retryCount++)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        await Task.Delay(_config.RetryDelay, cancellationToken);
                    }

                    response = await _httpClient.SendAsync(request, cancellationToken);

                    // 如果是成功的响应或者客户端错误（4xx），直接返回
                    if (response.IsSuccessStatusCode ||
                        (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                    {
                        return response;
                    }
                }
                catch (Exception ex) when (retryCount < _config.MaxRetryCount)
                {
                    lastException = ex;
                    continue;
                }
            }

            return response ?? throw lastException ?? new HttpRequestException("Request failed after all retries");
        }

        private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage httpResponse, DateTime requestTime)
        {
            var response = new ApiResponse<T>
            {
                Code = (int)httpResponse.StatusCode,
                IsSuccess = httpResponse.IsSuccessStatusCode,
                Headers = ConvertHeaders(httpResponse.Headers),
                RequestTime = requestTime,
                ResponseTime = DateTime.UtcNow - requestTime
            };

            if (typeof(T) == typeof(byte[]))
            {
                response.Data = (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
            }
            else
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (response.IsSuccess)
                {
                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        try
                        {
                            if (typeof(T) == typeof(string))
                            {
                                response.Data = (T)(object)responseContent;
                            }
                            else
                            {
                                var data = JsonSerializer.Deserialize<Response<T>>(responseContent, _jsonOptions);
                                response.Data = data.Result;
                            }
                        }
                        catch (JsonException ex)
                        {
                            response.Data = default;
                        }
                    }
                }
                else
                {
                    Response<object> result = JsonSerializer.Deserialize<Response<object>>(responseContent, _jsonOptions);

                    response.Msg = $"HTTP Error {httpResponse.StatusCode}: {result.Msg}";
                }
            }

            return response;
        }

        private Dictionary<string, IEnumerable<string>> ConvertHeaders(HttpHeaders headers)
        {
            var result = new Dictionary<string, IEnumerable<string>>();

            foreach (var header in headers)
            {
                result[header.Key] = header.Value;
            }

            return result;
        }
        #endregion

        #region IDisposable 实现
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                    _httpClientHandler?.Dispose();
                    _semaphore?.Dispose();
                }
                _disposed = true;
            }
        }
        #endregion
    }


    public class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        // 支持多种日期格式
        private static readonly string[] Formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy-MM-dd",
            "MM/dd/yyyy HH:mm:ss",
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",  // RFC 1123
            "ddd, dd MMM yyyy HH:mm:ss 'UTC'",
            "yyyy-MM-dd'T'HH:mm:ss.fffK",       // ISO 8601 with timezone
            "yyyy-MM-dd'T'HH:mm:ssK"
        };

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                // 1. 如果是数字（Unix时间戳）
                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (reader.TryGetInt64(out long unixTime))
                    {
                        // 判断是秒还是毫秒
                        if (unixTime > 10000000000) // 毫秒
                            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
                        else // 秒
                            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                    }
                    else if (reader.TryGetDouble(out double unixTimeDouble))
                    {
                        unixTime = (long)unixTimeDouble;
                        if (unixTime > 10000000000)
                            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
                        else
                            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                    }
                }

                // 2. 如果是字符串
                if (reader.TokenType == JsonTokenType.String)
                {
                    string dateString = reader.GetString();

                    if (string.IsNullOrEmpty(dateString))
                        return DateTime.MinValue;

                    // 先尝试标准解析
                    if (DateTime.TryParse(dateString,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out DateTime date))
                    {
                        return date;
                    }

                    // 尝试解析为 Unix 时间戳字符串
                    if (long.TryParse(dateString, out long timestamp))
                    {
                        if (timestamp > 10000000000)
                            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                        else
                            return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                    }

                    // 尝试各种格式
                    foreach (var format in Formats)
                    {
                        if (DateTime.TryParseExact(dateString, format,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out date))
                        {
                            return date;
                        }
                    }

                    // 如果还是失败，返回最小时间或抛出异常
                    return DateTime.MinValue;
                }

                // 3. 其他类型无法转换
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                // 记录日志，但返回默认值
                Console.WriteLine($"DateTime parsing failed: {ex.Message}");
                return DateTime.MinValue;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            // 输出为 ISO 8601 格式
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

}
