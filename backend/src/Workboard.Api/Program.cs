using Workboard.Api;

var builder = WebApplication.CreateBuilder(args);

await builder.AddApi();

builder.Services.AddOpenApi("v1");
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")))
    .WithName("GetHealth");

app.MapControllers();

app.Run();

internal sealed record HealthResponse(string Status);

public partial class Program;

