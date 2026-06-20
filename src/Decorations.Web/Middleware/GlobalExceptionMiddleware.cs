using System.Net;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Decorations.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception exception)
            {
                Log.Error(
                    exception,
                    "GlobalExceptionMiddleware.InvokeAsync - Excepción no controlada. Path: {Path} Method: {Method}",
                    context.Request.Path,
                    context.Request.Method);

                await this.HandleUnhandledExceptionAsync(context, exception);
            }
        }

        private async Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            bool isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjaxRequest)
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Se produjo un error interno en el servidor.\"}");
                return;
            }

            context.Response.Redirect("/Home/Error");
        }
    }
}
