using Microsoft.AspNetCore.Mvc.Testing;

namespace DNET.Backend.Api.Tests;

public class BaseApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;

    public BaseApiTests(WebApplicationFactory<Program> factory)
    {
        Client = factory.CreateClient();
    }
}