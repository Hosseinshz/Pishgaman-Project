using System.Collections.Concurrent;
using System.Net;

namespace Pishgaman_Project.Models
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _maxRequests;
        private readonly int _timeWindowSeconds;
        private static ConcurrentDictionary<string, RequestInfo> _requestLog = new ConcurrentDictionary<string, RequestInfo>();

        public RateLimitingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _maxRequests = int.Parse(configuration["RateLimitingSettings:MaxRequests"]);
            _timeWindowSeconds = int.Parse(configuration["RateLimitingSettings:TimeWindowSeconds"]);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                if (IsRateLimited(ipAddress))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    await context.Response.WriteAsync(@"<div class=""text-center"" style=""color:red;text-align:center"">Rate limit exceeded. Try again later.</div>");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsRateLimited(string ipAddress)
        {
            var now = DateTime.UtcNow;

            // Use AddOrUpdate to ensure that the dictionary is updated atomically
            var requestInfo = _requestLog.AddOrUpdate(ipAddress,
                new RequestInfo
                {
                    RequestCount = 1,
                    LastRequestTime = now
                },
                (key, existingRequestInfo) =>
                {
                    // If the time window has passed, reset the request count
                    if ((now - existingRequestInfo.LastRequestTime).TotalSeconds > _timeWindowSeconds)
                    {
                        existingRequestInfo.RequestCount = 1;
                        existingRequestInfo.LastRequestTime = now;
                    }
                    else
                    {
                        existingRequestInfo.RequestCount++;
                    }

                    return existingRequestInfo;
                });

            // Return true if the request count exceeds the maximum allowed requests
            return requestInfo.RequestCount > _maxRequests;
        }
    }

    public class RequestInfo
    {
        public int RequestCount { get; set; }
        public DateTime LastRequestTime { get; set; }
    }
}
