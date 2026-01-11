using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace AqiChartServer.WebApi.Helper
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var result = context.Exception switch
            {
                ArgumentException ex => new { code = 400, msg = ex.Message },
                _ => new { code = 500, msg = "系统异常" }
            };

            context.Result = new JsonResult(result);
            context.ExceptionHandled = true;
        }
    }
}
