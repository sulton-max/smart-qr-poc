using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Probes the Dapper-reads / EF-writes seam: can an entity materialized by Dapper be attached to EF and
// tracked correctly, and where does the original-values snapshot come from? Run against a throwaway
// Postgres database — see README.md.

var connectionString = args.FirstOrDefault()
    ?? Environment.GetEnvironmentVariable("DATASEAM_DB")
    ?? "Host=localhost;Port=5432;Database=smartqr_dataseam;Username=postgres;Password=postgres";

await ResetSchemaAsync(connectionString);

Probe1_DapperReadThenAttach(connectionString);
Probe2_MutateBeforeAttach(connectionString);
Probe3_MutateAfterAttach(connectionString);
Probe4_AttachThenReload(connectionString);
Probe5_EfNoTrackingBaseline(connectionString);
await Probe6_JsonbColumn(connectionString);

// ── Probe 1 — Dapper read → Attach: what state, and what does EF think the original was? ───────────
static void Probe1_DapperReadThenAttach(string cs)
{
    Console.WriteLine("\n-- 1 · Dapper read -> Attach (clean) --");

    using var db = new Db(cs);
    using var conn = new NpgsqlConnection(cs);
    var code = conn.QuerySingle<Code>("select id, name, mode from codes where id = 1");

    db.Attach(code);
    var entry = db.Entry(code);
    Console.WriteLine($"  state after Attach      : {entry.State}");
    Console.WriteLine($"  OriginalValues[Mode]    : {entry.OriginalValues["Mode"]}");
}

// ── Probe 2 — the claim under test: mutate BEFORE attaching ────────────────────────────────────────
static void Probe2_MutateBeforeAttach(string cs)
{
    Console.WriteLine("\n-- 2 · mutate, THEN Attach (the suspected trap) --");

    using var db = new Db(cs);
    using var conn = new NpgsqlConnection(cs);
    var code = conn.QuerySingle<Code>("select id, name, mode from codes where id = 1");

    code.Mode = "dynamic";              // mutate while detached
    db.Attach(code);

    var entry = db.Entry(code);
    Console.WriteLine($"  state                   : {entry.State}");
    Console.WriteLine($"  OriginalValues[Mode]    : {entry.OriginalValues["Mode"]}   (db holds 'static')");
    Console.WriteLine($"  CurrentValues[Mode]     : {entry.CurrentValues["Mode"]}");
    Console.WriteLine($"  SaveChanges would write : {(entry.State == EntityState.Modified ? "YES" : "NO - the edit is LOST")}");
}

// ── Probe 3 — the correct order: attach first, mutate after ────────────────────────────────────────
static void Probe3_MutateAfterAttach(string cs)
{
    Console.WriteLine("\n-- 3 · Attach, THEN mutate --");

    using var db = new Db(cs);
    using var conn = new NpgsqlConnection(cs);
    var code = conn.QuerySingle<Code>("select id, name, mode from codes where id = 1");

    db.Attach(code);
    code.Mode = "dynamic";

    var entry = db.Entry(code);
    Console.WriteLine($"  state                   : {entry.State}");
    Console.WriteLine($"  Original / Current      : {entry.OriginalValues["Mode"]} / {entry.CurrentValues["Mode"]}");
    Console.WriteLine($"  SaveChanges would write : {(entry.State == EntityState.Modified ? "YES" : "NO")}");
}

// ── Probe 4 — can the true original be recovered after a bad attach? ───────────────────────────────
static void Probe4_AttachThenReload(string cs)
{
    Console.WriteLine("\n-- 4 · recovering the true original (GetDatabaseValues) --");

    using var db = new Db(cs);
    using var conn = new NpgsqlConnection(cs);
    var code = conn.QuerySingle<Code>("select id, name, mode from codes where id = 1");

    code.Mode = "dynamic";
    db.Attach(code);

    var stored = db.Entry(code).GetDatabaseValues();
    Console.WriteLine($"  GetDatabaseValues[Mode] : {stored?["Mode"]}   (a SECOND query)");
    Console.WriteLine($"  OriginalValues[Mode]    : {db.Entry(code).OriginalValues["Mode"]}");
}

// ── Probe 5 — EF's own AsNoTracking, for contrast ──────────────────────────────────────────────────
static void Probe5_EfNoTrackingBaseline(string cs)
{
    Console.WriteLine("\n-- 5 · EF AsNoTracking baseline --");

    using var db = new Db(cs);
    var code = db.Codes.AsNoTracking().Single(c => c.Id == 1);
    code.Mode = "dynamic";

    var entry = db.Entry(code);
    Console.WriteLine($"  state                   : {entry.State}");
    Console.WriteLine($"  OriginalValues[Mode]    : {entry.OriginalValues["Mode"]}   (db holds 'static')");
    Console.WriteLine($"  tracked entries         : {db.ChangeTracker.Entries<Code>().Count()}");
}

// ── Probe 6 — the jsonb column: does Dapper materialize what EF's converter expects? ───────────────
static async Task Probe6_JsonbColumn(string cs)
{
    Console.WriteLine("\n-- 6 · jsonb column across the seam --");

    await using var conn = new NpgsqlConnection(cs);
    var raw = await conn.QuerySingleAsync<string>("select rules::text from codes where id = 1");
    Console.WriteLine($"  Dapper raw jsonb        : {raw}");

    using var db = new Db(cs);
    var viaEf = db.Codes.AsNoTracking().Single(c => c.Id == 1);
    Console.WriteLine($"  EF converted            : {viaEf.Rules.Count} rule(s), first = {viaEf.Rules.FirstOrDefault()}");
    Console.WriteLine("  -> a Dapper-read entity needs the same converter applied by hand.");
}

static async Task ResetSchemaAsync(string cs)
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.ExecuteAsync("""
        drop table if exists codes;
        create table codes (
            id    bigint primary key,
            name  text  not null,
            mode  text  not null,
            rules jsonb not null
        );
        insert into codes (id, name, mode, rules)
        values (1, 'Probe', 'static', '["one"]'::jsonb);
        """);
}

/// <summary>Stands in for the real code entity — the two members the probes mutate plus a jsonb column.</summary>
internal sealed class Code
{
    public long Id { get; set; }

    public string Name { get; set; } = "";

    public string Mode { get; set; } = "";

    public List<string> Rules { get; set; } = [];
}

/// <summary>The probe's EF context — maps <see cref="Code"/> onto the throwaway schema.</summary>
internal sealed class Db(string connectionString) : DbContext
{
    /// <summary>The codes under probe.</summary>
    public DbSet<Code> Codes => Set<Code>();

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseNpgsql(connectionString);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder model)
    {
        var code = model.Entity<Code>();
        code.ToTable("codes");
        code.Property(entity => entity.Id).HasColumnName("id");
        code.Property(entity => entity.Name).HasColumnName("name");
        code.Property(entity => entity.Mode).HasColumnName("mode");
        code.Property(entity => entity.Rules)
            .HasColumnName("rules")
            .HasColumnType("jsonb")
            .HasConversion(
                rules => System.Text.Json.JsonSerializer.Serialize(rules, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, (System.Text.Json.JsonSerializerOptions?)null)!);
    }
}
