using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

public sealed class StatblockValidationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient client;

    public StatblockValidationTests(ApiWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task IncompleteDraftCanBePersisted()
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload(
                $"Brouillon incomplet {Guid.NewGuid():N}",
                "daggerheart",
                new JsonObject()));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InvalidCommonFieldReturnsStandardProblemAndIsNotPersisted()
    {
        var countBefore = await CountStatblocks();
        var response = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload("   ", "daggerheart", new JsonObject()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "/problems/invalid-request",
            problem.RootElement.GetProperty("type").GetString());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(
            "identity.name",
            out _));
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));

        Assert.Equal(countBefore, await CountStatblocks());
    }

    [Theory]
    [InlineData("daggerheart", "{\"tier\":0}", "systemData.tier")]
    [InlineData("dungeonsAndDragons", "{\"rulesVersion\":\"5e\"}", "systemData.rulesVersion")]
    [InlineData("drawSteel", "{\"organization\":\"gang\"}", "systemData.organization")]
    [InlineData("dc20", "{\"statistics\":{\"physicalDefense\":[10,15]}}", "systemData.statistics.physicalDefense")]
    public async Task InvalidPresentSystemFieldReturns422AndIsNotPersisted(
        string system,
        string systemDataJson,
        string expectedErrorPath)
    {
        var uniqueName = $"SB invalide {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload(
                uniqueName,
                system,
                JsonNode.Parse(systemDataJson)!.AsObject()));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "/problems/invalid-system-data",
            problem.RootElement.GetProperty("type").GetString());
        Assert.Equal(422, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(
            expectedErrorPath,
            out _));

        using var list = JsonDocument.Parse(await client.GetStringAsync(
            $"/api/v1/statblocks?query={Uri.EscapeDataString(uniqueName)}"));
        Assert.Empty(list.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task PublishingRequiresCompleteSystemDataButDoesNotAlterDraftOnFailure()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload(
                $"Publication progressive {Guid.NewGuid():N}",
                "daggerheart",
                new JsonObject()));
        createResponse.EnsureSuccessStatusCode();
        var etag = createResponse.Headers.ETag!.Tag;
        using var created = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetGuid();

        using var incompleteRequest = UpdateRequest(
            id,
            etag,
            UpdatePayload("published", new JsonObject()));
        var incompleteResponse = await client.SendAsync(incompleteRequest);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, incompleteResponse.StatusCode);
        using var problem = JsonDocument.Parse(
            await incompleteResponse.Content.ReadAsStringAsync());
        var errors = problem.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("systemData.tier", out _));
        Assert.True(errors.TryGetProperty("systemData.statistics", out _));

        var unchangedResponse = await client.GetAsync($"/api/v1/statblocks/{id:D}");
        unchangedResponse.EnsureSuccessStatusCode();
        Assert.Equal(etag, unchangedResponse.Headers.ETag!.Tag);
        using var unchanged = JsonDocument.Parse(
            await unchangedResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "draft",
            unchanged.RootElement.GetProperty("metadata").GetProperty("status").GetString());

        var completeSystemData = new JsonObject
        {
            ["tier"] = 1,
            ["role"] = "Minion",
            ["statistics"] = new JsonObject
            {
                ["difficulty"] = 9,
                ["hitPoints"] = 1,
                ["stress"] = 1
            },
            ["features"] = new JsonArray()
        };
        using var completeRequest = UpdateRequest(
            id,
            etag,
            UpdatePayload("published", completeSystemData));
        var completeResponse = await client.SendAsync(completeRequest);

        completeResponse.EnsureSuccessStatusCode();
        using var published = JsonDocument.Parse(
            await completeResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "published",
            published.RootElement.GetProperty("metadata").GetProperty("status").GetString());
    }

    [Fact]
    public async Task TalesOfTheValiantDraftIsAllowedButPublicationWaitsForItsSchema()
    {
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/statblocks",
            CreatePayload(
                $"TotV progressif {Guid.NewGuid():N}",
                "talesOfTheValiant",
                new JsonObject()));
        createResponse.EnsureSuccessStatusCode();
        var etag = createResponse.Headers.ETag!.Tag;
        using var created = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("id").GetGuid();

        using var publishRequest = UpdateRequest(
            id,
            etag,
            UpdatePayload("published", new JsonObject(), "talesOfTheValiant"));
        var publishResponse = await client.SendAsync(publishRequest);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, publishResponse.StatusCode);
        using var problem = JsonDocument.Parse(
            await publishResponse.Content.ReadAsStringAsync());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(
            "systemData",
            out _));
    }

    [Fact]
    public async Task MalformedJsonReturnsStandard400Problem()
    {
        using var content = new StringContent("{", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/statblocks", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "/problems/invalid-request",
            problem.RootElement.GetProperty("type").GetString());
        Assert.True(problem.RootElement.TryGetProperty("errors", out _));
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    private static object CreatePayload(
        string name,
        string system,
        JsonObject systemData) => new
        {
            schemaVersion = "1.0",
            system,
            identity = new
            {
                name,
                subtitle = (string?)null,
                aliases = (string[]?)null
            },
            description = (string?)null,
            tags = Array.Empty<string>(),
            source = new
            {
                origin = "manual",
                sourceSystem = (string?)null,
                sourceStatblockId = (Guid?)null,
                reference = (string?)null
            },
            notes = (string?)null,
            systemData
        };

    private async Task<int> CountStatblocks()
    {
        var list = await client.GetFromJsonAsync<JsonObject>("/api/v1/statblocks");
        return list!["items"]!.AsArray().Count;
    }

    private static object UpdatePayload(
        string status,
        JsonObject systemData,
        string system = "daggerheart") => new
        {
            schemaVersion = "1.0",
            system,
            identity = new
            {
                name = "Publication progressive",
                subtitle = (string?)null,
                aliases = (string[]?)null
            },
            description = (string?)null,
            tags = Array.Empty<string>(),
            source = new
            {
                origin = "manual",
                sourceSystem = (string?)null,
                sourceStatblockId = (Guid?)null,
                reference = (string?)null
            },
            status,
            notes = (string?)null,
            systemData
        };

    private static HttpRequestMessage UpdateRequest(
        Guid id,
        string etag,
        object payload)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/v1/statblocks/{id:D}")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        return request;
    }
}
