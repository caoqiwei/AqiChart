using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Windows;

namespace AqiChart.Client.HttpClient
{
    public class ApiClientExamples
    {
        private readonly ApiClient _client;
        private const string BaseUrl = "https://jsonplaceholder.typicode.com/";

        public ApiClientExamples()
        {
            // 获取默认单例实例
            _client = ApiClient.Instance;

            // 配置客户端
            _client.SetBaseUrl(BaseUrl);
            _client.SetTimeout(TimeSpan.FromSeconds(60));

            // 添加默认请求头
            _client.AddDefaultHeader("X-App-Name", "WPF Demo App");
            _client.AddDefaultHeader("X-App-Version", "1.0.0");
        }

        #region 用户相关操作
        /// <summary>
        /// 获取所有用户
        /// </summary>
        public async Task<List<User>?> GetAllUsersAsync()
        {
            var response = await _client.GetAsync<List<User>>("users");

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("获取用户列表失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var response = await _client.GetAsync<User>($"users/{userId}");

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            if (response.Code == 404)
            {
                MessageBox.Show($"用户 {userId} 不存在", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowError($"获取用户 {userId} 失败", response.Msg);
            }

            return null;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        public async Task<User?> CreateUserAsync(CreateUserRequest request)
        {
            var config = new RequestConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "X-Request-ID", Guid.NewGuid().ToString() }
                }
            };

            var response = await _client.PostAsync<User>("users", request, config);

            if (response.IsSuccess && response.Data != null)
            {
                MessageBox.Show($"用户创建成功！ID: {response.Data.Id}", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return response.Data;
            }

            ShowError("创建用户失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task<User?> UpdateUserAsync(int userId, Dictionary<string, object> updates)
        {
            var response = await _client.PutAsync<User>($"users/{userId}", updates);

            if (response.IsSuccess && response.Data != null)
            {
                MessageBox.Show("用户更新成功！", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return response.Data;
            }

            ShowError("更新用户失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var result = MessageBox.Show($"确定要删除用户 {userId} 吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return false;

            var response = await _client.DeleteAsync<object>($"users/{userId}");

            if (response.IsSuccess)
            {
                MessageBox.Show("用户删除成功！", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            ShowError("删除用户失败", response.Msg);
            return false;
        }
        #endregion

        #region Todo相关操作
        /// <summary>
        /// 获取用户的所有待办事项
        /// </summary>
        public async Task<List<TodoItem>?> GetUserTodosAsync(int userId)
        {
            var parameters = new
            {
                userId = userId
            };

            var response = await _client.GetAsync<List<TodoItem>>("todos", parameters);

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("获取待办事项失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 创建待办事项
        /// </summary>
        public async Task<TodoItem?> CreateTodoAsync(CreateTodoRequest request)
        {
            var response = await _client.PostAsync<TodoItem>("todos", request);

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("创建待办事项失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 更新待办事项完成状态
        /// </summary>
        public async Task<TodoItem?> UpdateTodoStatusAsync(int todoId, bool completed)
        {
            var updates = new
            {
                completed = completed
            };

            var response = await _client.PatchAsync<TodoItem>($"todos/{todoId}", updates);

            if (response.IsSuccess && response.Data != null)
            {
                var status = completed ? "完成" : "未完成";
                MessageBox.Show($"待办事项已标记为{status}！", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return response.Data;
            }

            ShowError("更新待办事项失败", response.Msg);
            return null;
        }
        #endregion

        #region 帖子相关操作
        /// <summary>
        /// 获取用户的所有帖子
        /// </summary>
        public async Task<List<Post>?> GetUserPostsAsync(int userId)
        {
            var parameters = new
            {
                userId = userId
            };

            var response = await _client.GetAsync<List<Post>>("posts", parameters);

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("获取帖子失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 获取帖子的所有评论
        /// </summary>
        public async Task<List<Comment>?> GetPostCommentsAsync(int postId)
        {
            var response = await _client.GetAsync<List<Comment>>($"posts/{postId}/comments");

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("获取评论失败", response.Msg);
            return null;
        }
        #endregion

        #region 文件操作
        /// <summary>
        /// 上传文件
        /// </summary>
        public async Task<UploadResponse?> UploadFileAsync(string filePath, string description = "")
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在！", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                var additionalData = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(description))
                {
                    additionalData["description"] = description;
                }

                // 使用扩展方法上传文件
                var response = await _client.UploadFileAsync<UploadResponse>(
                    "upload",
                    filePath,
                    "file",
                    additionalData);

                if (response.IsSuccess && response.Data != null)
                {
                    MessageBox.Show($"文件上传成功！\n大小: {response.Data.FileSize} bytes",
                        "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    return response.Data;
                }

                ShowError("文件上传失败", response.Msg);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"上传文件时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public async Task<bool> DownloadFileAsync(string url, string savePath)
        {
            try
            {
                // 直接下载文件到指定路径
                await _client.DownloadFileAsync(url, savePath);

                MessageBox.Show($"文件下载成功！\n保存到: {savePath}", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载文件时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region 批量操作
        /// <summary>
        /// 批量获取用户信息
        /// </summary>
        public async Task<List<User>> BatchGetUsersAsync(List<int> userIds)
        {
            var userTasks = new List<Func<Task<ApiResponse<User>>>>();

            foreach (var userId in userIds)
            {
                userTasks.Add(() => _client.GetAsync<User>($"users/{userId}"));
            }

            var responses = await Task.WhenAll(userTasks.Select(task => task()));

            var users = new List<User>();
            foreach (var response in responses)
            {
                if (response.IsSuccess && response.Data != null)
                {
                    users.Add(response.Data);
                }
            }

            return users;
        }

        /// <summary>
        /// 批量更新待办事项状态
        /// </summary>
        public async Task<int> BatchUpdateTodosAsync(List<int> todoIds, bool completed)
        {
            var updateTasks = new List<Task<ApiResponse<TodoItem>>>();

            foreach (var todoId in todoIds)
            {
                var updates = new { completed = completed };
                updateTasks.Add(_client.PatchAsync<TodoItem>($"todos/{todoId}", updates));
            }

            var responses = await Task.WhenAll(updateTasks);

            var successCount = responses.Count(r => r.IsSuccess);
            MessageBox.Show($"成功更新 {successCount}/{todoIds.Count} 个待办事项", "批量操作",
                MessageBoxButton.OK, MessageBoxImage.Information);

            return successCount;
        }
        #endregion

        #region 搜索和分页
        /// <summary>
        /// 搜索用户
        /// </summary>
        public async Task<List<User>?> SearchUsersAsync(SearchRequest request)
        {
            var response = await _client.GetAsync<List<User>>("users/search", request);

            if (response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }

            ShowError("搜索用户失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 分页获取待办事项
        /// </summary>
        public async Task<PaginatedResponse<TodoItem>?> GetTodosPaginatedAsync(int page = 1, int pageSize = 20)
        {
            var parameters = new
            {
                _page = page,
                _limit = pageSize
            };

            var response = await _client.GetAsync<List<TodoItem>>("todos", parameters);

            if (response.IsSuccess && response.Data != null)
            {
                // 注意：这个API没有返回总数，实际项目中应该从响应头获取
                return new PaginatedResponse<TodoItem>
                {
                    Items = response.Data,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = response.Data.Count // 这只是示例
                };
            }

            ShowError("获取待办事项失败", response.Msg);
            return null;
        }
        #endregion

        #region 认证相关
        /// <summary>
        /// 模拟登录
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(string username, string password)
        {
            var loginData = new
            {
                username = username,
                password = password
            };

            var response = await _client.PostAsync<LoginResponse>("auth/login", loginData);

            if (response.IsSuccess && response.Data != null)
            {
                // 保存Token
                _client.SetBearerToken(response.Data.AccessToken);
                MessageBox.Show("登录成功！", "成功",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return response.Data;
            }

            ShowError("登录失败", response.Msg);
            return null;
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            _client.ClearBearerToken();
            MessageBox.Show("已登出！", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        public async Task<bool> RefreshTokenAsync(string refreshToken)
        {
            var refreshData = new
            {
                refreshToken = refreshToken
            };

            var response = await _client.PostAsync<LoginResponse>("auth/refresh", refreshData);

            if (response.IsSuccess && response.Data != null)
            {
                _client.SetBearerToken(response.Data.AccessToken);
                return true;
            }

            return false;
        }
        #endregion

        #region 高级用法
        /// <summary>
        /// 使用扩展方法链式调用
        /// </summary>
        public async Task<User?> GetUserWithChainingAsync(int userId)
        {
            var response = await _client
                .WithTimeout(TimeSpan.FromSeconds(10))
                .WithHeader("X-Client-Type", "WPF")
                .WithHeader("X-Request-Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .GetAsync<User>($"users/{userId}");

            return response.Data;
        }

        /// <summary>
        /// 下载大文件并显示进度
        /// </summary>
        public async Task<bool> DownloadLargeFileAsync(string url, string savePath,
            IProgress<double>? progress = null)
        {
            try
            {
                // 获取文件大小
                var headResponse = await _client.GetAsync<object>(url, null,
                    new RequestConfig { Headers = new Dictionary<string, string> { { "Range", "bytes=0-0" } } });

                long? totalSize = null;
                if (headResponse.Headers != null &&
                    headResponse.Headers.TryGetValue("Content-Range", out var rangeValues))
                {
                    var range = rangeValues.FirstOrDefault();
                    if (range != null && range.Contains('/'))
                    {
                        var parts = range.Split('/');
                        if (parts.Length == 2 && long.TryParse(parts[1], out var size))
                        {
                            totalSize = size;
                        }
                    }
                }

                // 下载文件
                var streamResponse = await _client.GetStreamAsync(url);
                if (!streamResponse.IsSuccess || streamResponse.Data == null)
                    return false;

                using var stream = streamResponse.Data;
                using var fileStream = File.Create(savePath);

                var buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    // 报告进度
                    if (progress != null && totalSize.HasValue)
                    {
                        var percent = (double)totalRead / totalSize.Value;
                        progress.Report(percent);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"下载失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 带重试机制的请求
        /// </summary>
        public async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<ApiResponse<T>>> apiCall, int maxRetries = 3)
        {
            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry - 1)));
                        MessageBox.Show($"第 {retry} 次重试...", "提示",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    var response = await apiCall();

                    if (response.IsSuccess)
                    {
                        return response.Data;
                    }

                    // 如果是客户端错误，不重试
                    if (response.Code >= 400 && response.Code < 500)
                    {
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    if (retry == maxRetries)
                        throw;
                }
                catch (HttpRequestException)
                {
                    if (retry == maxRetries)
                        throw;
                }
            }

            return default;
        }
        #endregion

        #region 辅助方法
        private void ShowError(string title, string? message)
        {
            var fullMessage = $"{title}\n\n错误信息: {message ?? "未知错误"}";
            MessageBox.Show(fullMessage, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 获取API状态
        /// </summary>
        public async Task<ApiStatus> GetApiStatusAsync()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var response = await _client.GetAsync<object>("");
                var endTime = DateTime.UtcNow;

                return new ApiStatus
                {
                    IsOnline = response.IsSuccess,
                    ResponseTime = endTime - startTime,
                    StatusCode = response.Code,
                    LastChecked = DateTime.Now
                };
            }
            catch
            {
                return new ApiStatus
                {
                    IsOnline = false,
                    LastChecked = DateTime.Now
                };
            }
        }
        #endregion
    }

    #region 辅助类
    public class ApiStatus
    {
        public bool IsOnline { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public DateTime LastChecked { get; set; }

        public string StatusText => IsOnline ? "在线" : "离线";
        public string ResponseTimeText => IsOnline ? $"{ResponseTime.TotalMilliseconds:F0}ms" : "N/A";
    }

    public class DownloadProgress
    {
        public long BytesDownloaded { get; set; }
        public long? TotalBytes { get; set; }
        public double Percentage => TotalBytes.HasValue ? (double)BytesDownloaded / TotalBytes.Value * 100 : 0;
        public bool IsCompleted { get; set; }
        public string? Msg { get; set; }
    }
    #endregion

    #region 基础模型
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int UserId { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        [JsonIgnore]
        public string DisplayName => $"{Name} ({Username})";
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int PostId { get; set; }
    }
    #endregion

    #region 请求模型
    public class CreateTodoRequest
    {
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int UserId { get; set; }
    }

    public class UpdateTodoRequest
    {
        public string? Title { get; set; }
        public bool? Completed { get; set; }
    }

    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SearchRequest
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
    #endregion

    #region 响应模型
    public class ApiResult<T>
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult<T> Success(T data, string message = "Success")
        {
            return new ApiResult<T>
            {
                Code = 0,
                Message = message,
                Data = data
            };
        }

        public static ApiResult<T> Error(string message, int code = -1)
        {
            return new ApiResult<T>
            {
                Code = code,
                Message = message,
                Data = default
            };
        }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }

    public class UploadResponse
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
    #endregion

}
