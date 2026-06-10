using SmartQr.Redirect.Configurations;
using SmartQr.Common.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configure();

var app = builder.Build();
app.Configure();

// POC: create the database + schema at startup if missing (idempotent).
await app.Services.EnsureSmartQrDatabaseAsync();

Console.WriteLine("🚀 SmartQr.Redirect starting...");
Console.WriteLine("   Hot path: GET /{slug} → rule eval → 302");
Console.WriteLine("   Health:   /health");

app.Run();
