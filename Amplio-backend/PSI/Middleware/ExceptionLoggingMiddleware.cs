using System.Net;

namespace PSI.Middleware
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                LogException(ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogException(Exception ex)
        {
            string log = $"{DateTime.Now}: {ex.GetType().Name} - {ex.Message}{Environment.NewLine}";
            File.AppendAllText("logs.txt", log);
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new
            {
                error = ex.Message,
                type = ex.GetType().Name
            };

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
