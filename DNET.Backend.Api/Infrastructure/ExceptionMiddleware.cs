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
        catch (BadRequestException badRequestException)
        {
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = badRequestException.WrongMessage,
                Status = badRequestException.WrongCode
            });

            context.Response.StatusCode = 400;
        }
        catch (NullReferenceException nullReferenceException)
        {
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = nullReferenceException.Message,
                Status = 404
            });

            context.Response.StatusCode = 404;
        }
        catch (Exception ex)
        {
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = ex.Message,
                Status = 500
            });

            context.Response.StatusCode = 500;
        }
    }
}