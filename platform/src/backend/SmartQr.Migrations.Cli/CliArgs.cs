namespace SmartQr.Migrations.Cli;

/// <summary>Minimal verb + <c>--flag value</c> + positional parser (no dependency for the POC CLI).</summary>
internal static class CliArgs
{
    public static (string Command, Dictionary<string, string> Flags, List<string> Positionals) Parse(string[] args)
    {
        var command = args.Length > 0 ? args[0] : "help";
        var flags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var positionals = new List<string>();

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var key = arg[2..];
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    flags[key] = args[++i];
                else
                    flags[key] = "true";
            }
            else
            {
                positionals.Add(arg);
            }
        }

        return (command, flags, positionals);
    }
}
