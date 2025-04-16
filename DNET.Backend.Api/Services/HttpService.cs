using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DNET.Backend.Api.Models;
using SendGrid;

namespace DNET.Backend.Api.Services;

public class HttpService : IHttpService
{
    
    private readonly IHttpClientFactory _httpClientFactory;
    private IConfiguration _configuration;

    
    public HttpService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    
    public async Task<MyHttpResponse> GetConfirmationCode(string email)
    {
        var httpClient = _httpClientFactory.CreateClient("MyHttpClient");

        var postResult = await httpClient
            .PostAsJsonAsync("email",
                new MyHttpRequest
                {
                    Email = email
                }
            );

        if (!postResult.IsSuccessStatusCode)
            throw new Exception("Error while calling webhook");

        var result = await postResult.Content.ReadFromJsonAsync<MyHttpResponse>();
        return result ?? throw new Exception();
    }

    public async Task SendMailAsync(string toEmail, string subject, string message)
    {
        var httpClient = _httpClientFactory.CreateClient("MyHttpClient");
        var sendGridApiKey = _configuration["SendGrid:ApiKey"];
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sendGridApiKey);
        
        var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.sendgrid.com/v3/mail/send"
        );

        requestMessage.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[] { new { email = toEmail } }, subject
                    }
                },
                content = new[]
                {
                    new { type = "text/html", value = message }
                },
                from = new { email = "zoltikovakira@gmail.com", name = "ivelmakina-kzholtikova" },
                reply_to = new { email = "zoltikovakira@gmail.com", name = "ivelmakina-kzholtikova" }
            }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.SendAsync(requestMessage);
        
        if (!response.IsSuccessStatusCode)
            throw new ServerException("Failed to send email.", 503);
    }
}
