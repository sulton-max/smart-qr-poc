using SmartQr.Api.Configurations;
using SmartQr.Common.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configure();

var app = builder.Build();
app.Configure();

// POC: create the database + schema at startup if missing (idempotent).
await app.Services.EnsureSmartQrDatabaseAsync();

Console.WriteLine("🚀 SmartQr.Api starting...");
Console.WriteLine("   Endpoints: POST /api/codes, GET /api/codes, GET /api/codes/{id}, GET /api/codes/{id}/image");
Console.WriteLine("   Health:    /health");

app.Run();
