using System.Runtime.CompilerServices;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke; // DatabaseProvider enum (shared with the bespoke migrator)
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace SmartQr.Tests.Integration.Harness;

/// <summary>Selects the test-DB provider for this integration suite — Postgres by default, matching CI.</summary>
/// <remarks>Flip the line below to in-memory SQLite for a container-free local loop.</remarks>
internal static class TestModuleInit
{
    [ModuleInitializer]
    internal static void Init()
    {
        // Postgres is the default (TestSetupOptions.Current.Database). To run this integration suite on SQLite,
        // uncomment:
        // TestSetupOptions.Current.Database = DatabaseProvider.Sqlite;
    }
}
