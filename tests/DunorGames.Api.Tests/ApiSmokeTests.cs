using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public void ApiAssemblyLoads()
    {
        Assert.NotNull(typeof(Program).Assembly);
    }

    [Fact]
    public async Task SwaggerUiIsAvailable()
    {
        var response = await client.GetAsync("/swagger/index.html");

        response.EnsureSuccessStatusCode();
        Assert.Contains(
            "DunorGames StatsBlock API",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApiDocumentIsAvailable()
    {
        using var document = JsonDocument.Parse(
            await client.GetStringAsync("/openapi/v1.json"));

        Assert.StartsWith(
            "3.1.",
            document.RootElement.GetProperty("openapi").GetString());
    }
}
