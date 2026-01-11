
namespace AqiChart.Client.Services
{
    /// <summary>
    /// 轮询任务执行状态
    /// </summary>
    public enum PollingTaskStatus
    {
        Stopped,
        Running,
        Paused,
        Faulted
    }

    /// <summary>
    /// 轮询任务配置
    /// </summary>
    public class PollingTaskConfig
    {
        /// <summary>
        /// 轮询间隔时间(默认为5分钟)
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 任务失败后重试间隔(默认为1分钟)
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// 最大重试次数(默认为3次)
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 是否立即执行第一次任务
        /// </summary>
        public bool RunImmediately { get; set; } = true;

        /// <summary>
        /// 任务执行超时时间（null表示不超时）
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// 是否在应用程序启动时自动启动
        /// </summary>
        public bool AutoStart { get; set; } = false;
    }

    /// <summary>
    /// 轮询任务执行结果
    /// </summary>
    public class PollingTaskResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan ExecutionDuration { get; set; }
    }

    /// <summary>
    /// 轮询任务事件参数
    /// </summary>
    public class PollingTaskEventArgs : EventArgs
    {
        public PollingTaskResult Result { get; set; }
        public PollingTaskStatus Status { get; set; }
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// 通用后台轮询任务服务
    /// </summary>
    public class BackgroundPollingService : IDisposable
    {
        #region 事件

        /// <summary>
        /// 任务开始执行事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> TaskStarted;

        /// <summary>
        /// 任务执行完成事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> TaskCompleted;

        /// <summary>
        /// 任务执行失败事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> TaskFailed;

        /// <summary>
        /// 任务状态变更事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> StatusChanged;

        /// <summary>
        /// 轮询服务启动事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> ServiceStarted;

        /// <summary>
        /// 轮询服务停止事件
        /// </summary>
        public event EventHandler<PollingTaskEventArgs> ServiceStopped;

        #endregion

        #region 私有字段

        private readonly Func<CancellationToken, Task<PollingTaskResult>> _taskFunc;
        private readonly PollingTaskConfig _config;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;
        private readonly SemaphoreSlim _operationLock = new SemaphoreSlim(1, 1);
        private PollingTaskStatus _status = PollingTaskStatus.Stopped;
        private int _retryCount;
        private DateTime _lastExecutionTime;

        #endregion

        #region 属性

        /// <summary>
        /// 当前状态
        /// </summary>
        public PollingTaskStatus Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnStatusChanged(new PollingTaskEventArgs
                    {
                        Status = value,
                        RetryCount = _retryCount
                    });
                }
            }
        }

        /// <summary>
        /// 配置信息
        /// </summary>
        public PollingTaskConfig Config => _config;

        /// <summary>
        /// 最后执行时间
        /// </summary>
        public DateTime LastExecutionTime => _lastExecutionTime;

        /// <summary>
        /// 当前重试次数
        /// </summary>
        public int CurrentRetryCount => _retryCount;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => Status == PollingTaskStatus.Running;

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused => Status == PollingTaskStatus.Paused;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建轮询服务
        /// </summary>
        /// <param name="taskFunc">要执行的任务函数</param>
        /// <param name="config">配置（可选）</param>
        public BackgroundPollingService(
            Func<CancellationToken, Task<PollingTaskResult>> taskFunc,
            PollingTaskConfig config = null)
        {
            _taskFunc = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
            _config = config ?? new PollingTaskConfig();
        }

        /// <summary>
        /// 创建轮询服务（简单版本）
        /// </summary>
        /// <param name="taskAction">要执行的任务</param>
        /// <param name="interval">间隔时间</param>
        public BackgroundPollingService(
            Func<Task> taskAction,
            TimeSpan interval) : this(
                async ct =>
                {
                    await taskAction();
                    return new PollingTaskResult { IsSuccess = true };
                },
                new PollingTaskConfig { Interval = interval })
        {
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 启动轮询服务
        /// </summary>
        public async Task StartAsync()
        {
            await _operationLock.WaitAsync();
            try
            {
                if (Status != PollingTaskStatus.Stopped)
                {
                    throw new InvalidOperationException($"服务当前状态为 {Status}，无法启动");
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _pollingTask = ExecutePollingLoopAsync(_cancellationTokenSource.Token);
                Status = PollingTaskStatus.Running;

                OnServiceStarted(new PollingTaskEventArgs
                {
                    Status = Status
                });
            }
            finally
            {
                _operationLock.Release();
            }
        }

        /// <summary>
        /// 停止轮询服务
        /// </summary>
        public async Task StopAsync()
        {
            await _operationLock.WaitAsync();
            try
            {
                if (Status == PollingTaskStatus.Stopped)
                {
                    return;
                }

                _cancellationTokenSource?.Cancel();

                if (_pollingTask != null)
                {
                    try
                    {
                        await _pollingTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // 正常取消，忽略异常
                    }
                    catch (Exception ex)
                    {
                        // 记录异常但不抛出
                        System.Diagnostics.Debug.WriteLine($"停止轮询服务时发生异常: {ex.Message}");
                    }
                    finally
                    {
                        _pollingTask = null;
                    }
                }

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                Status = PollingTaskStatus.Stopped;

                OnServiceStopped(new PollingTaskEventArgs
                {
                    Status = Status
                });
            }
            finally
            {
                _operationLock.Release();
            }
        }

        /// <summary>
        /// 暂停轮询服务
        /// </summary>
        public async Task PauseAsync()
        {
            await _operationLock.WaitAsync();
            try
            {
                if (Status != PollingTaskStatus.Running)
                {
                    throw new InvalidOperationException($"服务当前状态为 {Status}，无法暂停");
                }

                Status = PollingTaskStatus.Paused;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        /// <summary>
        /// 恢复轮询服务
        /// </summary>
        public async Task ResumeAsync()
        {
            await _operationLock.WaitAsync();
            try
            {
                if (Status != PollingTaskStatus.Paused)
                {
                    throw new InvalidOperationException($"服务当前状态为 {Status}，无法恢复");
                }

                Status = PollingTaskStatus.Running;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        /// <summary>
        /// 手动触发一次任务执行
        /// </summary>
        public async Task<PollingTaskResult> TriggerManualExecutionAsync()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("服务未启动或已停止");
            }

            return await ExecuteTaskAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// 获取服务状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            return $"""
                服务状态: {Status}
                最后执行时间: {_lastExecutionTime:yyyy-MM-dd HH:mm:ss}
                当前重试次数: {_retryCount}
                轮询间隔: {_config.Interval}
                最大重试次数: {_config.MaxRetryCount}
                """;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行轮询循环
        /// </summary>
        private async Task ExecutePollingLoopAsync(CancellationToken cancellationToken)
        {
            // 如果配置了立即执行，先执行一次
            if (_config.RunImmediately)
            {
                await ExecuteWithRetryAsync(cancellationToken);
            }

            // 开始轮询循环
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 等待指定的间隔时间，但可以被取消
                    await Task.Delay(_config.Interval, cancellationToken);

                    // 检查服务是否暂停
                    if (Status == PollingTaskStatus.Paused)
                    {
                        // 如果暂停，等待恢复
                        await WaitWhilePausedAsync(cancellationToken);
                        continue;
                    }

                    // 执行任务
                    await ExecuteWithRetryAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // 正常取消，退出循环
                    break;
                }
                catch (Exception ex)
                {
                    // 记录异常但不中断循环
                    OnTaskFailed(new PollingTaskEventArgs
                    {
                        Result = new PollingTaskResult
                        {
                            IsSuccess = false,
                            Message = "轮询循环发生异常",
                            Exception = ex,
                            ExecutionTime = DateTime.Now
                        },
                        Status = PollingTaskStatus.Faulted
                    });

                    // 等待一段时间后重试
                    await Task.Delay(_config.RetryInterval, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 等待直到服务恢复运行
        /// </summary>
        private async Task WaitWhilePausedAsync(CancellationToken cancellationToken)
        {
            while (Status == PollingTaskStatus.Paused && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        /// <summary>
        /// 带重试机制的任务执行
        /// </summary>
        private async Task ExecuteWithRetryAsync(CancellationToken cancellationToken)
        {
            _retryCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await ExecuteTaskAsync(cancellationToken);

                    if (result.IsSuccess)
                    {
                        _retryCount = 0; // 成功执行后重置重试计数
                        return;
                    }

                    // 执行失败，检查重试次数
                    _retryCount++;

                    if (_retryCount >= _config.MaxRetryCount)
                    {
                        OnTaskFailed(new PollingTaskEventArgs
                        {
                            Result = result,
                            Status = PollingTaskStatus.Faulted,
                            RetryCount = _retryCount
                        });

                        // 达到最大重试次数，等待一段时间后继续
                        await Task.Delay(_config.RetryInterval * 2, cancellationToken);
                        _retryCount = 0;
                    }
                    else
                    {
                        // 等待重试间隔
                        await Task.Delay(_config.RetryInterval, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw; // 重新抛出取消异常
                }
                catch (Exception ex)
                {
                    _retryCount++;

                    var result = new PollingTaskResult
                    {
                        IsSuccess = false,
                        Message = "任务执行发生异常",
                        Exception = ex,
                        ExecutionTime = DateTime.Now
                    };

                    OnTaskFailed(new PollingTaskEventArgs
                    {
                        Result = result,
                        Status = PollingTaskStatus.Faulted,
                        RetryCount = _retryCount
                    });

                    if (_retryCount >= _config.MaxRetryCount)
                    {
                        await Task.Delay(_config.RetryInterval * 2, cancellationToken);
                        _retryCount = 0;
                    }
                    else
                    {
                        await Task.Delay(_config.RetryInterval, cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// 执行单个任务
        /// </summary>
        private async Task<PollingTaskResult> ExecuteTaskAsync(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            OnTaskStarted(new PollingTaskEventArgs
            {
                Status = Status,
                RetryCount = _retryCount
            });

            try
            {
                PollingTaskResult result;

                if (_config.Timeout.HasValue)
                {
                    // 使用超时机制执行任务
                    var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(_config.Timeout.Value);

                    try
                    {
                        result = await _taskFunc(timeoutCts.Token);
                    }
                    finally
                    {
                        timeoutCts.Dispose();
                    }
                }
                else
                {
                    // 无超时执行任务
                    result = await _taskFunc(cancellationToken);
                }

                result.ExecutionTime = startTime;
                result.ExecutionDuration = DateTime.Now - startTime;
                _lastExecutionTime = startTime;

                OnTaskCompleted(new PollingTaskEventArgs
                {
                    Result = result,
                    Status = Status,
                    RetryCount = _retryCount
                });

                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // 重新抛出取消异常
            }
            catch (Exception ex)
            {
                var result = new PollingTaskResult
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Exception = ex,
                    ExecutionTime = startTime,
                    ExecutionDuration = DateTime.Now - startTime
                };

                _lastExecutionTime = startTime;

                OnTaskCompleted(new PollingTaskEventArgs
                {
                    Result = result,
                    Status = Status,
                    RetryCount = _retryCount
                });

                return result;
            }
        }

        #endregion

        #region 事件触发方法

        protected virtual void OnTaskStarted(PollingTaskEventArgs e)
        {
            TaskStarted?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(PollingTaskEventArgs e)
        {
            TaskCompleted?.Invoke(this, e);
        }

        protected virtual void OnTaskFailed(PollingTaskEventArgs e)
        {
            TaskFailed?.Invoke(this, e);
        }

        protected virtual void OnStatusChanged(PollingTaskEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        protected virtual void OnServiceStarted(PollingTaskEventArgs e)
        {
            ServiceStarted?.Invoke(this, e);
        }

        protected virtual void OnServiceStopped(PollingTaskEventArgs e)
        {
            ServiceStopped?.Invoke(this, e);
        }

        #endregion

        #region IDisposable 实现

        private bool _disposed = false;

        public async void Dispose()
        {
            await DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await StopAsync();
                _operationLock?.Dispose();
                _disposed = true;
            }
        }

        ~BackgroundPollingService()
        {
            DisposeAsync().AsTask().Wait();
        }

        #endregion
    }
}