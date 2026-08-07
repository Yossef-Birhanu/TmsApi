using Microsoft.AspNetCore.Mvc.Filters;
namespace Filters
{
    public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter
    {
     

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Code to execute before the action executes
            // For example, log the request details
            var route = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;
          logger.LogInformation("TMS API Call: {Method} {Route}", method, route);

        
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Code to execute after the action executes
            // For example, log the response details
            var status= context.HttpContext.Response.StatusCode;
            logger.LogInformation("TMS API Response: {StatusCode}", status);
        }
    }
}