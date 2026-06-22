using System.Reflection;
using Microsoft.Extensions.Configuration;
using SmartQr.Common.Extensions;

namespace SmartQr.Common.Configuration;

/// <summary>Generic config loader — binds an IConfiguration section (name = class name), then overlays env vars for <see cref="EnvironmentVariableAttribute"/> properties.</summary>
public static class ConfigurationLoader
{
    /// <summary>Builds a config object of type <typeparamref name="T"/> from appsettings and env-var overlay.</summary>
    public static T Load<T>(IConfiguration configuration) where T : class, new()
    {
        var sectionName = typeof(T).Name;
        var instance = configuration.GetSection(sectionName).Get<T>() ?? new T();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<EnvironmentVariableAttribute>();
            if (attr is null)
                continue;

            var envValue = Environment.GetEnvironmentVariable(attr.EnvVarName).NullIfEmpty();

            if (envValue is not null && prop.CanWrite)
            {
                prop.SetValue(instance, envValue);
                continue;
            }

            if (attr.Required)
            {
                var currentValue = prop.GetValue(instance) as string;
                if (string.IsNullOrWhiteSpace(currentValue))
                    throw new InvalidOperationException(
                        $"Configuration '{prop.Name}' not found. " +
                        $"Set env var '{attr.EnvVarName}' or appsettings section '{sectionName}:{prop.Name}'.");
            }
        }

        return instance;
    }
}
