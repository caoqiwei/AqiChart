using Newtonsoft.Json;
using Serilog;
using System.Reflection.Metadata;

namespace AqiChartServer.WebApi.Helper
{
    /// <summary>
    /// 自定义异常处理类
    /// </summary>
    public class ExceptionFilter
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public ExceptionFilter(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            bool isCatched = false;
            try
            {
                await _next(context);
            }
            catch (Exception ex) //发生异常
            {
                //自定义业务异常
                if (ex is MyException exception)
                {
                    Log.Information(ex.Message);
                    context.Response.StatusCode = exception.GetCode();
                }
                //未知异常
                else
                {
                    Log.Error(ex.Message, ex);
                    context.Response.StatusCode = 500;
                }
                await HandleExceptionAsync(context, context.Response.StatusCode, ex.Message);
                isCatched = true;
            }
            finally
            {
                if (!isCatched && context.Response.StatusCode != 200)//未捕捉过并且状态码不为200
                {
                    Log.Information($"Response StatusCode:{context.Response.StatusCode},Path:{context.Request.Path}");
                    string msg = context.Response.StatusCode switch
                    {
                        401 => "未授权",
                        404 => "未找到服务",
                        502 => "请求错误",
                        _ => "未知错误",
                    };
                    await HandleExceptionAsync(context, context.Response.StatusCode, msg);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statusCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static Task HandleExceptionAsync(HttpContext context, int statusCode, string msg)
        {
            var data = new { status = 1, code = statusCode, msg };
            context.Response.ContentType = "application/json;charset=utf-8";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(data));
        }

    }
}
