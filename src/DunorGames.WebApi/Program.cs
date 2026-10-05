using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DunorGames.WebApi;
using DunorGames.Business.Statblocks;
using DunorGames.Data.Models;
using DunorGames.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var allowedWebOrigin = builder.Configuration["Cors:AllowedWebOrigin"]
    ?? throw new InvalidOperationException("Cors:AllowedWebOrigin must be configured.");
var swaggerEnabled = builder.Configuration.GetValue(
    "Swagger:Enabled",
    builder.Environment.IsDevelopment());
var useDevelopmentInMemoryDatabase =
    builder.Environment.IsDevelopment() &&
    string.Equals(
        builder.Configuration["Persistence:Provider"],
        "InMemory",
        StringComparison.OrdinalIgnoreCase);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            context.ProblemDetails.Type = ApiProblemTypes.InternalError;
            context.ProblemDetails.Title = "Erreur interne de l’API";
            context.ProblemDetails.Detail =
                "Une erreur inattendue s’est produite. Utilisez le traceId pour le diagnostic.";
        }
    };
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ApiProblemResponses.InvalidModelState;
});
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
if (useDevelopmentInMemoryDatabase)
{
    builder.Services.AddSingleton<DevelopmentRowVersionInterceptor>();
    builder.Services.AddDbContext<DunorGamesDbContext>((provider, options) =>
    {
        options
            .UseInMemoryDatabase("DunorGamesDevelopment")
            .AddInterceptors(
                provider.GetRequiredService<DevelopmentRowVersionInterceptor>());
    });
}
else
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=localhost;Database=DunorGamesDb;Trusted_Connection=True;TrustServerCertificate=True;";

    builder.Services.AddDbContext<DunorGamesDbContext>(
        options => options.UseSqlServer(connectionString));
}
builder.Services.AddScoped<IStatblockRepository, EfStatblockRepository>();
builder.Services.AddScoped<IStatblockStore, EfStatblockStore>();
builder.Services.AddSingleton<IStatblockRequestValidator, StatblockRequestValidator>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DunorGamesWeb", policy =>
    {
        policy.WithOrigins(allowedWebOrigin)
            .WithExposedHeaders("ETag")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (useDevelopmentInMemoryDatabase)
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<DunorGamesDbContext>()
        .Database.EnsureCreated();
}

app.UseExceptionHandler();

if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DunorGames StatsBlock API v1");
        options.DocumentTitle = "DunorGames StatsBlock API";
    });
}

app.UseHttpsRedirection();
app.UseCors("DunorGamesWeb");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
