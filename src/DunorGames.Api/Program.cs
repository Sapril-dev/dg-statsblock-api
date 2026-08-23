var builder = WebApplication.CreateBuilder(args);

var allowedWebOrigin = builder.Configuration["Cors:AllowedWebOrigin"]
    ?? throw new InvalidOperationException("Cors:AllowedWebOrigin must be configured.");

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("DunorGamesWeb");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
