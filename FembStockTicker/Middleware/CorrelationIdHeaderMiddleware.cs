
namespace FembStockTicker.Middleware
{
    public class CorrelationIdHeaderMiddleware
    {
        private const string HeaderName = "correlationId";
        private readonly RequestDelegate _next;

        public CorrelationIdHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(HeaderName))
            {
                context.Request.Headers[HeaderName] = System.Guid.NewGuid().ToString();
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                {
                    context.Response.Headers.Append(HeaderName, context.Request.Headers[HeaderName].ToString());
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}