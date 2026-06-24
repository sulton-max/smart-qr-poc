using System.Runtime.CompilerServices;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke; // DatabaseProvider enum (shared with the bespoke migrator)
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace SmartQr.Tests.Integration.Harness;

/// <summary>The single place that selects the test-DB provider for this integration suite (the <see cref="SmartQrTestDb"/> repository / config-store tests). Default is Postgres (the fidelity baseline, matching CI). Flip the line below to in-memory SQLite for a fast, container-free local loop. (The E2E tier boots the real hosts and is Postgres-only.)</summary>
internal static class TestModuleInit
{
    [ModuleInitializer]
    internal static void Init()
    {
        // Postgres is the default (TestSetupOptions.Current.Database). To run this integration suite on SQLite, uncomment:
        // TestSetupOptions.Current.Database = DatabaseProvider.Sqlite;
    }
}
