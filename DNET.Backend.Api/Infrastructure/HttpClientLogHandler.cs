using System.Diagnostics;

namespace DNET.Backend.Api.Infrastructure;

public class HttpClientLogHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLogHandler> _logger;

    public HttpClientLogHandler(ILogger<HttpClientLogHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("HTTP {Method} {RequestUri} started at {Timestamp}", 
            request.Method, request.RequestUri, DateTime.Now);
        
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("HTTP {Method} {RequestUri} responded with status code {StatusCode} in {ElapsedMilliseconds}ms at {Timestamp}",
                request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds, DateTime.Now);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "HTTP {Method} {RequestUri} failed in {ElapsedMilliseconds}ms at {Timestamp}",
                request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds, DateTime.Now);

            throw;
        }
    }
}
