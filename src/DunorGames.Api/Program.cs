var builder = WebApplication.CreateBuilder(args);

var allowedWebOrigin = builder.Configuration["Cors:AllowedWebOrigin"]
    ?? throw new InvalidOperationException("Cors:AllowedWebOrigin must be configured.");
var swaggerEnabled = builder.Configuration.GetValue(
    "Swagger:Enabled",
    builder.Environment.IsDevelopment());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DunorGamesWeb", policy =>
    {
        policy.WithOrigins(allowedWebOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

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
