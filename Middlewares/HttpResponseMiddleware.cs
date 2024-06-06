namespace Net8.Example.HttpLogger.Middlewares
{
    public class HttpResponseMiddleware
    {
        private readonly ILogger<HttpResponseMiddleware> _logger;
        private readonly RequestDelegate _next;

        public HttpResponseMiddleware(RequestDelegate next, ILogger<HttpResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation($"Response: {context.Response.StatusCode}");
                _logger.LogInformation($"Response Body: {responseBodyText}");

                // Copy the contents of the new memory stream (which contains the response) to the original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HttpResponseMiddleware");
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
