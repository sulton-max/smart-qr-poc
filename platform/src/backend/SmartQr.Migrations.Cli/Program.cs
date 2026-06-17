using System.CommandLine;
using Dapper;
using SmartQr.Common.Persistence.Migrations;
using SmartQr.Migrations.Cli;

// smart-qr-migrate — dev CLI over the SQL migrator engine (proving ground for the wow-two SDK migrator).
// Command tree + global options live in CliCommands; command bodies + DI wiring live in CliRunner.

DefaultTypeMap.MatchNamesWithUnderscores = true; // Dapper snake_case → PascalCase for migration_history reads

// Disable the built-in handler so action exceptions surface as a clean "✗ message" below, not a stack trace.
var configuration = new InvocationConfiguration { EnableDefaultExceptionHandler = false };

try
{
    return await CliCommands.Build().Parse(args).InvokeAsync(configuration);
}
// Exit codes: 0 success · 1 validation (drift, bad config / missing file) · 2 execution (DB / apply failure, guard tripped).
catch (Exception ex)
{
    Console.Error.WriteLine($"✗ {ex.Message}");
    return ex switch
    {
        MigrationDriftException => 1,
        DirectoryNotFoundException or FileNotFoundException => 1,
        _ => 2,
    };
}
