namespace SmartQr.Common.Configuration;

/// <summary>Marks a settings property to be overlaid from an environment variable by <see cref="ConfigurationLoader"/>.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EnvironmentVariableAttribute(string envVarName, bool required = false) : Attribute
{
    /// <summary>The environment variable name to read.</summary>
    public string EnvVarName { get; } = envVarName;

    /// <summary>Whether the value must be present after binding and env overlay (throws if missing).</summary>
    public bool Required { get; } = required;
}
