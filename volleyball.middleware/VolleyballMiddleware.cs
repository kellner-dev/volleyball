using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using volleyball.middleware.message;
using volleyball.middleware.queue;

namespace volleyball.middleware
{
    public class VolleyballMiddleware
    {
        private readonly RequestDelegate _next;

        public VolleyballMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();

            try
            {
                await _next(httpContext);
                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                var statusCode = httpContext.Response.StatusCode;
                LogVolley(httpContext, statusCode, elapsedMs);
            }
            catch (Exception)
                // Still get the developer page
                when (LogVolley(httpContext, 500, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp())))
            {

            }
        }

        bool LogVolley(HttpContext httpContext, int statusCode, double elapsedMs)
        {
            //TODO: Consider a kill switch for logging here

            //TODO: Use a Factory here
            var message = new AnyVolleyballMessage(){
                RequestMethod = httpContext.Request.Method,
                RequestPath = GetPath(httpContext),
                StatusCode = statusCode,
                Elapsed = elapsedMs
            };

            //TODO: Use a factory here
            IVolleyballQueue queue = new RabbitMQVolleyBallQueue();
            queue.Publish(message);

            return false;
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
        
        static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}
