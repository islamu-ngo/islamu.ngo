// ABOUTME: Resolves the application assembly version reported by the managed control-plane contract.
// ABOUTME: Keeps this non-handler helper outside handler namespaces so handler visibility rules stay precise.

using System.Reflection;

namespace Explore.Application.Features.Management;

internal static class ManagementVersionResolver
{
    public static string EventVersion { get; } = ResolveEventVersion();

    private static string ResolveEventVersion()
    {
        var assembly = typeof(ManagementVersionResolver).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}
