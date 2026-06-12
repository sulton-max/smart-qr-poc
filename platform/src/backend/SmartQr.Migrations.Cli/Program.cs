using Dapper;
using SmartQr.Common.Persistence.Migrations;
using SmartQr.Migrations.Cli;

// smart-qr-migrate — dev CLI over the SQL migrator engine (proving ground for the wow-two SDK migrator).
// Dependency-free arg handling for the POC; the SDK extraction swaps in System.CommandLine + Spectre.Console.

DefaultTypeMap.MatchNamesWithUnderscores = true; // Dapper snake_case → PascalCase for migration_history reads

var (command, flags, positionals) = CliArgs.Parse(args);

string Conn() =>
    flags.GetValueOrDefault("connection")
    ?? Environment.GetEnvironmentVariable("SMARTQR_DB_CONNECTION")
    ?? "Host=localhost;Port=5432;Database=smartqr;Username=postgres;Password=postgres";

string SqlDir() =>
    flags.GetValueOrDefault("sql-dir")
    ?? Environment.GetEnvironmentVariable("SMARTQR_SQL_DIR")
    ?? MigrationsPathResolver.Resolve();

try
{
    return command switch
    {
        "status" => await StatusAsync(),
        "apply" => await ApplyAsync(),
        "rollback" => await RollbackAsync(),
        "verify" => await VerifyAsync(),
        "new" => NewMigration(),
        "merge" => MergeDev(),
        "help" or "--help" or "-h" => Help(),
        _ => Unknown(command),
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine($"✗ {ex.Message}");
    return 1;
}

async Task<int> StatusAsync()
{
    var status = await MigratorFactory.Build(Conn(), SqlDir(), allowRollback: true).GetStatusAsync();

    Console.WriteLine($"Applied ({status.Applied.Count}):");
    foreach (var row in status.Applied)
        Console.WriteLine($"  ✓ {row.Ordinal:D3}-{row.Name}   [{row.AppliedBy} @ {row.AppliedAt:u}, {row.ExecutionMs}ms]");

    Console.WriteLine($"Pending ({status.Pending.Count}):");
    foreach (var migration in status.Pending)
        Console.WriteLine($"  • {migration.Label}");

    if (status.Drifted.Count > 0)
    {
        Console.WriteLine($"Drift ({status.Drifted.Count}):");
        foreach (var migration in status.Drifted)
            Console.WriteLine($"  ! {migration.Label}");
    }

    if (status.Orphaned.Count > 0)
        Console.WriteLine($"Orphaned (in DB, not in source): {string.Join(", ", status.Orphaned.Select(o => o.ToString("D3")))}");

    return 0;
}

async Task<int> ApplyAsync()
{
    await DatabaseBootstrap.EnsureDatabaseExistsAsync(Conn());
    var applied = await MigratorFactory.Build(Conn(), SqlDir(), allowRollback: true).ApplyPendingAsync("cli");

    Console.WriteLine(applied.Count == 0
        ? "✓ Up to date — nothing to apply."
        : $"✓ Applied {applied.Count}: {string.Join(", ", applied)}");
    return 0;
}

async Task<int> RollbackAsync()
{
    int? target = flags.TryGetValue("to", out var raw) && int.TryParse(raw, out var ordinal) ? ordinal : null;
    var rolledBack = await MigratorFactory.Build(Conn(), SqlDir(), allowRollback: true).RollbackAsync(target);

    Console.WriteLine(rolledBack.Count == 0
        ? "Nothing to roll back."
        : $"✓ Rolled back {rolledBack.Count}: {string.Join(", ", rolledBack)}");
    return 0;
}

async Task<int> VerifyAsync()
{
    var runner = MigratorFactory.Build(Conn(), SqlDir(), allowRollback: true);

    if (flags.ContainsKey("repair"))
    {
        var repaired = await runner.RepairAsync();
        Console.WriteLine(repaired.Count == 0
            ? "✓ No drift to repair."
            : $"✓ Repaired checksums: {string.Join(", ", repaired)}");
        return 0;
    }

    var status = await runner.GetStatusAsync();
    if (status.Drifted.Count == 0)
    {
        Console.WriteLine("✓ No drift — every applied migration matches its source.");
        return 0;
    }

    Console.Error.WriteLine(
        $"✗ Drift in {status.Drifted.Count}: {string.Join(", ", status.Drifted.Select(m => m.Label))}. " +
        "Revert the edits, or run 'verify --repair' to accept them.");
    return 1;
}

int NewMigration()
{
    if (positionals.Count == 0)
    {
        Console.Error.WriteLine("Usage: new <name>");
        return 1;
    }

    var name = Slugify(positionals[0]);
    var devDir = Path.Combine(SqlDir(), "Dev");
    Directory.CreateDirectory(devDir);

    var path = Path.Combine(devDir, $"{name}.sql");
    if (File.Exists(path))
    {
        Console.Error.WriteLine($"✗ {path} already exists.");
        return 1;
    }

    File.WriteAllText(path,
        $"-- Dev migration: {name}\n" +
        "-- Iterate freely. Promote to a numbered Apply/Rollback pair with: smart-qr-migrate merge\n" +
        "-- Put '-- @no-transaction' as the FIRST line if this must run outside a transaction.\n\n");

    Console.WriteLine($"✓ Created {path}");
    return 0;
}

int MergeDev()
{
    var root = SqlDir();
    var devDir = Path.Combine(root, "Dev");
    if (!Directory.Exists(devDir))
    {
        Console.WriteLine("No Dev/ folder — nothing to merge.");
        return 0;
    }

    var devFiles = Directory.GetFiles(devDir, "*.sql").OrderBy(f => f, StringComparer.Ordinal).ToList();
    if (devFiles.Count == 0)
    {
        Console.WriteLine("No Dev/*.sql — nothing to merge.");
        return 0;
    }

    var next = NextOrdinal(root);
    foreach (var file in devFiles)
    {
        var name = Slugify(Path.GetFileNameWithoutExtension(file));
        var folder = Path.Combine(root, $"{next:D3}-{name}");
        Directory.CreateDirectory(folder);

        File.Copy(file, Path.Combine(folder, "Apply.sql"));
        File.WriteAllText(Path.Combine(folder, "Rollback.sql"),
            $"-- Rollback for {next:D3}-{name}. Write the inverse of Apply.sql (dev/test only).\n");
        File.Delete(file);

        Console.WriteLine($"✓ Promoted Dev/{Path.GetFileName(file)} → {next:D3}-{name}/");
        next++;
    }

    return 0;
}

static int NextOrdinal(string root)
{
    var max = 0;
    foreach (var dir in Directory.GetDirectories(root))
    {
        var match = System.Text.RegularExpressions.Regex.Match(Path.GetFileName(dir), @"^(\d{3})-");
        if (match.Success)
            max = Math.Max(max, int.Parse(match.Groups[1].Value));
    }

    return max + 1;
}

static string Slugify(string value) =>
    new string(value.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');

int Help()
{
    Console.WriteLine(
        """
        smart-qr-migrate — SQL migrator CLI (dev)

        Commands:
          status              Show applied / pending / drift / orphaned
          apply               Ensure the DB exists, then apply pending migrations
          rollback [--to N]   Roll back the latest migration (or down to ordinal N). Dev only.
          verify [--repair]   Exit 1 on checksum drift; --repair re-records edited checksums
          new <name>          Scaffold Dev/<name>.sql
          merge               Promote Dev/*.sql → numbered NNN-name/{Apply,Rollback}.sql

        Options:
          --connection <str>  Postgres connection (else SMARTQR_DB_CONNECTION, else localhost/smartqr)
          --sql-dir <path>    Migrations dir (else SMARTQR_SQL_DIR, else auto-discovered)
        """);
    return 0;
}

int Unknown(string name)
{
    Console.Error.WriteLine($"Unknown command '{name}'. Run 'help'.");
    return 1;
}
