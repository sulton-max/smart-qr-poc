using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace SmartQr.Migrations.Tests.Harness;

/// <summary>xUnit collection sharing one drop-schema Postgres fixture across every migrator test class.</summary>
[CollectionDefinition(Name)]
public sealed class MigratorCollection : ICollectionFixture<MigratorPostgresFixture>
{
    /// <summary>The collection name every migrator test class joins.</summary>
    public const string Name = "smart-qr-migrator";
}
