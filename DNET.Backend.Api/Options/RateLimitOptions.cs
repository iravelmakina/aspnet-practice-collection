namespace DNET.Backend.Api.Options;

public class RateLimitOptions
{
    public int RequestsPerMinute { get; set; }
    public int WindowMinutes { get; set; }
}