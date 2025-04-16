using System.Diagnostics;

namespace DNET.Backend.Api.Infrastructure;

public class HttpClientLogHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Console.WriteLine("\nHTTP {0} {1} started at {2}\n", request.Method, request.RequestUri, DateTime.Now);
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            Console.WriteLine("\nHTTP {0} {1} responded {2} in {3}ms at {4}\n", request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds, DateTime.Now);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine("HTTP {0} {1} failed in {2}ms",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
