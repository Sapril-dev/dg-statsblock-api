using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

public sealed class StatblockCrudTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient client;

    public StatblockCrudTests(ApiWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteCrudCycleUsesEtagsAndListFilters()
    {
        var uniqueName = $"Bandit API {Guid.NewGuid():N}";
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload(uniqueName));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var firstEtag = Assert.IsType<System.Net.Http.Headers.EntityTagHeaderValue>(
            createResponse.Headers.ETag);

        using var createdDocument = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var id = createdDocument.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("daggerheart", createdDocument.RootElement.GetProperty("system").GetString());
        Assert.Equal("draft", createdDocument.RootElement
            .GetProperty("metadata")
            .GetProperty("status")
            .GetString());

        var getResponse = await client.GetAsync($"/api/v1/statblocks/{id:D}");
        getResponse.EnsureSuccessStatusCode();
        Assert.Equal(firstEtag.Tag, getResponse.Headers.ETag?.Tag);
        using var getDocument = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Bandit des rues",
            getDocument.RootElement
                .GetProperty("identity")
                .GetProperty("aliases")[0]
                .GetString());

        var listResponse = await client.GetAsync(
            $"/api/v1/statblocks?system=daggerheart&status=draft&query={uniqueName}");
        listResponse.EnsureSuccessStatusCode();
        using var listDocument = JsonDocument.Parse(
            await listResponse.Content.ReadAsStringAsync());
        var item = Assert.Single(listDocument.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(id, item.GetProperty("id").GetGuid());

        using var updateRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/v1/statblocks/{id:D}")
        {
            Content = JsonContent.Create(UpdatePayload($"{uniqueName} modifié"))
        };
        updateRequest.Headers.TryAddWithoutValidation("If-Match", firstEtag.Tag);
        var updateResponse = await client.SendAsync(updateRequest);

        updateResponse.EnsureSuccessStatusCode();
        var secondEtag = Assert.IsType<System.Net.Http.Headers.EntityTagHeaderValue>(
            updateResponse.Headers.ETag);
        Assert.NotEqual(firstEtag.Tag, secondEtag.Tag);

        using var staleRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/v1/statblocks/{id:D}")
        {
            Content = JsonContent.Create(UpdatePayload("Écriture périmée"))
        };
        staleRequest.Headers.TryAddWithoutValidation("If-Match", firstEtag.Tag);
        Assert.Equal(
            HttpStatusCode.PreconditionFailed,
            (await client.SendAsync(staleRequest)).StatusCode);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/statblocks/{id:D}");
        deleteRequest.Headers.TryAddWithoutValidation("If-Match", secondEtag.Tag);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.SendAsync(deleteRequest)).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/v1/statblocks/{id:D}")).StatusCode);
    }

    private static object CreatePayload(string name) => new
    {
        schemaVersion = "1.0",
        system = "daggerheart",
        identity = new
        {
            name,
            subtitle = (string?)null,
            aliases = new[] { "Bandit des rues" }
        },
        description = (string?)null,
        tags = new[] { "test-api" },
        source = new
        {
            origin = "manual",
            sourceSystem = (string?)null,
            sourceStatblockId = (Guid?)null,
            reference = (string?)null
        },
        notes = (string?)null,
        systemData = new { }
    };

    private static object UpdatePayload(string name) => new
    {
        schemaVersion = "1.0",
        system = "daggerheart",
        identity = new { name, subtitle = "Tier 1 Minion", aliases = Array.Empty<string>() },
        description = "Un brouillon modifié.",
        tags = new[] { "test-api", "updated" },
        source = new
        {
            origin = "manual",
            sourceSystem = (string?)null,
            sourceStatblockId = (Guid?)null,
            reference = (string?)null
        },
        status = "draft",
        notes = (string?)null,
        systemData = new { tier = 1, role = "Minion" }
    };
}
