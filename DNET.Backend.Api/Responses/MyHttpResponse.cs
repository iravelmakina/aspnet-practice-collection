using System.Text.Json.Serialization;

namespace DNET.Backend.Api.Models;

public class MyHttpResponse
{
    [JsonPropertyName("confirmation_code")]
    public string ConfirmationCode { get; set; } = string.Empty;
    
    [JsonPropertyName("confirmation_url")]
    public string ConfirmationUrl { get; set; } = string.Empty;
}
