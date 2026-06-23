using System.Runtime.CompilerServices;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace SmartQr.Tests.Harness;

/// <summary>The single place that selects the test-DB provider for the whole suite. Default is Postgres (the fidelity baseline, matching CI). Flip the line below to in-memory SQLite for a fast, container-free local loop.</summary>
internal static class TestModuleInit
{
    [ModuleInitializer]
    internal static void Init()
    {
        // Postgres is the default (TestSetupOptions.Current.Database). To run the whole suite on SQLite, uncomment:
        // TestSetupOptions.Current.Database = DatabaseProvider.Sqlite;
    }
}
