using System.Text;

namespace Net8.Example.HttpLogger.Middlewares
{
    public class Log_Request_Response_Middleware
    {
        private readonly ILogger<Log_Request_Response_Middleware> _logger;
        private readonly RequestDelegate _next;

        public Log_Request_Response_Middleware(RequestDelegate next, ILogger<Log_Request_Response_Middleware> logger)
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
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;
                await _next(context);
                try
                {
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
}
