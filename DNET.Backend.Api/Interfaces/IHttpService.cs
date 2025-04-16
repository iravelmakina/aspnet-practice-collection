using DNET.Backend.Api.Models;
using SendGrid;

namespace DNET.Backend.Api.Services;

public interface IHttpService
{
    public Task<MyHttpResponse> GetConfirmationCode(string email);
    public Task SendMailAsync(string toEmail, string subject, string body);
}