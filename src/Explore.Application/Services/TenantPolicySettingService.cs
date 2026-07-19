// ABOUTME: Service implementation for managing tenant policy settings with instance-level delegation constraints.
// ABOUTME: Partial class root with constructor, constants, and shared static resolvers (Resolve*/Deserialize*/Normalize*).

using System.Text.Json;
using Explore.Application.Contracts.Persistence;
using Explore.Application.Contracts.Services;

namespace Explore.Application.Services;

public partial class TenantPolicySettingService : ITenantPolicySettingService
{
    private const string DefaultBrandDisplayName = "";
    private const string DefaultPublicHomePage = "LandingPage";
    private const string DefaultCommunityGuidelinesContent =
        "# Community Guidelines\n\n" +
        "## Our Community\n\n" +
        "ISLAMU is a space for learning, sharing, and participating respectfully. To support a positive experience for everyone, we ask all community members and contributors to follow these guidelines.\n\n" +
        "## Content Standards\n\n" +
        "**Accuracy and Transparency**\n" +
        "- Share complete and accurate information.\n" +
        "- Clearly identify the source and purpose of contributed content.\n" +
        "- Correct misleading or outdated information promptly.\n\n" +
        "**Appropriate Content**\n" +
        "- Contributions must be relevant to the community and align with ISLAMU's purpose.\n" +
        "- Titles, descriptions, and links must be truthful and not misleading.\n" +
        "- Do not post duplicate, promotional, or spam content.\n\n" +
        "**Inclusive and Respectful Language**\n" +
        "- Use welcoming, inclusive language in content and communications.\n" +
        "- Content must not promote discrimination based on race, ethnicity, religion, gender, disability, or any other protected characteristic.\n" +
        "- Maintain respectful communication with other community members.\n\n" +
        "## Prohibited Content\n\n" +
        "The following content and behavior are not permitted on this platform:\n\n" +
        "- Content that promotes illegal activities or violates applicable laws\n" +
        "- Hateful, discriminatory, or violent content\n" +
        "- Harassment or targeted abuse of individuals or groups\n" +
        "- Deceptive, fraudulent, or misleading content\n" +
        "- Spam or commercially exploitative content\n\n" +
        "## Participation Guidelines\n\n" +
        "- Follow the applicable code of conduct and community rules.\n" +
        "- Be courteous when discussing different views or experiences.\n" +
        "- Respect privacy and do not share another person's information without permission.\n" +
        "- Respond constructively when moderators request a correction or clarification.\n\n" +
        "## Reporting Violations\n\n" +
        "If you encounter content or behavior that violates these guidelines, contact the site administrators. Reports are reviewed carefully and handled as confidentially as practical.\n\n" +
        "## Consequences\n\n" +
        "Violations may result in content removal, a warning, temporary suspension, or permanent removal from the platform for serious or repeated misconduct.";

    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly ITenantSettingRepository _tenantSettingRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ISettingMutationLock _mutationLock;

    public TenantPolicySettingService(
        ISystemSettingRepository systemSettingRepository,
        ITenantSettingRepository tenantSettingRepository,
        ITenantRepository tenantRepository,
        ISettingMutationLock mutationLock)
    {
        _systemSettingRepository = systemSettingRepository;
        _tenantSettingRepository = tenantSettingRepository;
        _tenantRepository = tenantRepository;
        _mutationLock = mutationLock;
    }

    private static bool ResolveBoolean(string? tenantOverrideValue, string? systemValue, bool fallback, bool allowTenantOverride)
    {
        if (allowTenantOverride && !string.IsNullOrWhiteSpace(tenantOverrideValue))
        {
            return DeserializeBoolean(tenantOverrideValue, fallback);
        }

        return DeserializeBoolean(systemValue, fallback);
    }

    private static string ResolveString(string? tenantOverrideValue, string? systemValue, string fallback, bool allowTenantOverride)
    {
        if (allowTenantOverride && !string.IsNullOrWhiteSpace(tenantOverrideValue))
        {
            return DeserializeString(tenantOverrideValue, fallback);
        }

        return DeserializeString(systemValue, fallback);
    }

    private static IReadOnlyList<string> ResolveStringList(
        string? tenantOverrideValue,
        string? systemValue,
        IReadOnlyList<string> fallback,
        bool allowTenantOverride)
    {
        if (allowTenantOverride && !string.IsNullOrWhiteSpace(tenantOverrideValue))
        {
            return DeserializeStringList(tenantOverrideValue, fallback);
        }

        return DeserializeStringList(systemValue, fallback);
    }

    private static bool DeserializeBoolean(string? rawValue, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        try
        {
            return JsonSerializer.Deserialize<bool>(rawValue);
        }
        catch
        {
            return bool.TryParse(rawValue, out var parsed) ? parsed : fallback;
        }
    }

    private static string DeserializeString(string? rawValue, string fallback)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        try
        {
            var deserialized = JsonSerializer.Deserialize<string>(rawValue);
            return deserialized ?? fallback;
        }
        catch
        {
            return rawValue.Trim('"');
        }
    }

    private static IReadOnlyList<string> DeserializeStringList(string? rawValue, IReadOnlyList<string> fallback)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        try
        {
            var deserialized = JsonSerializer.Deserialize<IReadOnlyList<string>>(rawValue);
            return NormalizeAiModelIds(deserialized ?? fallback);
        }
        catch
        {
            return NormalizeAiModelIds(rawValue.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));
        }
    }

    private static IReadOnlyList<string> NormalizeAiModelIds(params IEnumerable<string?>[] modelIdGroups)
    {
        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in modelIdGroups)
        {
            foreach (var modelId in group)
            {
                var trimmed = modelId?.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || !seen.Add(trimmed))
                {
                    continue;
                }

                normalized.Add(trimmed);
            }
        }

        return normalized;
    }

    private static int DeserializeInteger(string? rawValue, int fallback)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return fallback;
        }

        try
        {
            return JsonSerializer.Deserialize<int>(rawValue);
        }
        catch
        {
            return int.TryParse(rawValue.Trim('"'), out var parsed) ? parsed : fallback;
        }
    }

    private static string NormalizeHomePage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultPublicHomePage;
        }

        return value.Equals("LandingPage", StringComparison.OrdinalIgnoreCase)
            ? "LandingPage"
            : DefaultPublicHomePage;
    }

    private static string NormalizeOptionalHost(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant();
        normalized = normalized.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);
        normalized = normalized.Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
        return normalized.Trim('/').Trim();
    }

    private static string? NormalizeSubdomain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        normalized = normalized.Replace(" ", "-");
        normalized = new string(normalized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
