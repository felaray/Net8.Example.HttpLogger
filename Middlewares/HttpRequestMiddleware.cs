using System.Text;

namespace Net8.Example.HttpLogger.Middlewares
{
    public class HttpRequestMiddleware
    {
        private readonly ILogger<HttpRequestMiddleware> _logger;
        private readonly RequestDelegate _next;

        public HttpRequestMiddleware(RequestDelegate next, ILogger<HttpRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation($"Request: {context.Request.Path}");
                // Log the request content
                var request = context.Request;
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var requestBody = Encoding.UTF8.GetString(buffer);
                _logger.LogInformation($"Request Body: {requestBody}");
                request.Body.Seek(0, SeekOrigin.Begin);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HttpRequestMiddleware");
            }
            finally
            {
                await _next(context);
            }
        }
    }
}
