using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AqiChartServer.WebApi.Helper
{
    public class MyResultMiddleWare : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                //base.OnActionExecuted(context);
                return;
            }

            if (context.Result is ObjectResult)
            {
                var objectResult = context.Result as ObjectResult;
                if (objectResult != null && objectResult.Value == null)
                {
                    context.Result = new ObjectResult(new { status = 1, code = 30001, msg = "未找到数据" });
                }
                else
                {
                    context.Result = new ObjectResult(new { status = 1, code = 200, msg = "", result = objectResult.Value });
                }
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new { status = 1, code = 30001, msg = "未找到数据" });
            }
            else if (context.Result is ContentResult)
            {
                var contentResult = context.Result as ContentResult;
                context.Result = new ObjectResult(new { status = 1, code = 200, msg = "", result = contentResult.Content });
            }
            else if (context.Result is StatusCodeResult)
            {
                var statusCodeResult = context.Result as StatusCodeResult;
                context.Result = new ObjectResult(new { status = 1, code = statusCodeResult.StatusCode, msg = statusCodeResult.ToString() });
            }

            else if (context.Result is BadRequestResult)
            {
                if (context.Result is BadRequestResult badRequestResult)
                    context.Result = new ObjectResult(new
                    {
                        status = 1,
                        code = badRequestResult.StatusCode,
                        msg = "",
                        result = badRequestResult.StatusCode
                    });
            }
            else
            {
                context.Result = new ObjectResult(new { status = 1, code = 200, msg = "", result = context.Result });
            }

            base.OnActionExecuted(context);
        }
    }
}
