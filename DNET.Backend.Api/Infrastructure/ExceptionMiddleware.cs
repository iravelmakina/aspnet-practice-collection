using DNET.Backend.Api.Models;

namespace DNET.Backend.Api.Infrastructure;

public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ServerException e)
        {
            context.Response.StatusCode = e.WrongCode;

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = e.WrongMessage,
                Status = e.WrongCode
            });
        }
        catch (NullReferenceException nullReferenceException)
        {
            context.Response.StatusCode = 404;
            
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = nullReferenceException.Message,
                Status = 404
            });
        }
        catch (System.Exception ex)
        {
            context.Response.StatusCode = 500;

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message,
                Status = 500
            });
        }
    }
}