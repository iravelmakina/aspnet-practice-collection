using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Infrastructure;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ServerException e)
        {
            _logger.LogWarning("Server exception {StatusCode} occurred while processing {Path}: {Message}", 
                e.WrongCode, context.Request.Path, e.WrongMessage);
            
            context.Response.StatusCode = e.WrongCode;
            
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
        catch (NullReferenceException nullReferenceException)
        {
            _logger.LogWarning("NullReferenceException {StatusCode} occurred while processing {Path}: {Message}", 
                404, context.Request.Path, nullReferenceException.Message);
            
            context.Response.StatusCode = 404;
            
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = nullReferenceException.Message,
                Status = 404
            });
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning("Unexpected exception {StatusCode} occurred while processing {Path}: {Message}", 
                500, context.Request.Path, ex.Message);
            
            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message,
                Status = 500
            });
        }
    }
}