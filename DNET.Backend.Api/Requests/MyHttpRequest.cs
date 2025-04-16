using System.Text.Json.Serialization;

namespace DNET.Backend.Api.Models;

public class MyHttpRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
