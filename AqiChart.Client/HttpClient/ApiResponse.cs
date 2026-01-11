using System;

namespace AqiChart.Client.HttpClient
{
    /// <summary>
    /// API 响应包装类
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public bool IsSuccess {  get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }
        public Dictionary<string, IEnumerable<string>>? Headers { get; set; }
        public DateTime RequestTime { get; set; }
        public TimeSpan ResponseTime { get; set; }


    }

    /// <summary>
    /// Api 内部异常和业务问题处理响应类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T> : BaseModel
    {
        public T Result { get; set; }
    }

    public class Response : BaseModel
    {
        public string Result { get; set; }
    }

    public class BaseModel
    {
        public int Code { get; set; }
        public string Msg { get; set; }
    }
}
