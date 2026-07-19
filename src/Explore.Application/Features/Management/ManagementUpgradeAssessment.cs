// ABOUTME: Evaluates managed control-plane upgrade readiness and semantic-version compatibility.
// ABOUTME: Keeps reusable assessment logic outside CQRS handler namespaces.

using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Management;
using Explore.Application.Management;
using Explore.Domain;
using Explore.Domain.Enums;

namespace Explore.Application.Features.Management;

internal static class ManagementUpgradeAssessment
{
    public static void AddCommonBlockers(
        ICollection<ManagementUpgradeBlockerDto> blockers,
        ManagedControlPlaneRegistration? registration,
        bool bootstrapCompleted,
        DeploymentMode currentMode,
        ManagedEventHealthObservation health,
        DateTime observedAt)
    {
        if (registration?.Status != ManagedControlPlaneRegistrationStatus.Registered)
        {
            AddBlocker(
                blockers,
                "managed_registration_unavailable",
                "An active managed Control Plane registration is required.");
        }
        else
        {
            if (registration.ControlPlaneToEventCredentialExpiresAt <= observedAt)
            {
                AddBlocker(
                    blockers,
                    "control_plane_credential_expired",
                    "The inbound Control Plane credential has expired.");
            }

            if (registration.EventToControlPlaneCredentialExpiresAt <= observedAt)
            {
                AddBlocker(
                    blockers,
                    "event_callback_credential_expired",
                    "The ISLAMU callback credential has expired.");
            }

            if (!string.Equals(
                    registration.ManagementApiVersion,
                    ManagedControlPlaneContract.ManagementApiVersion,
                    StringComparison.Ordinal))
            {
                AddBlocker(
                    blockers,
                    "registration_contract_stale",
                    "The registered management API contract does not match this ISLAMU runtime.");
            }

            if (registration.DeploymentMode != currentMode)
            {
                AddBlocker(
                    blockers,
                    "deployment_mode_changed",
                    "The current ISLAMU deployment mode does not match the registered managed-mode identity.");
            }
        }

        if (!bootstrapCompleted)
        {
            AddBlocker(
                blockers,
                "instance_bootstrap_incomplete",
                "ISLAMU instance bootstrap must be complete before upgrade validation.");
        }

        if (!string.Equals(health.Status, "Healthy", StringComparison.Ordinal))
        {
            AddBlocker(
                blockers,
                "instance_health_not_ready",
                "Aggregate ISLAMU readiness must be healthy.");
        }
    }

    public static void AddPreflightVersionBlockers(
        ICollection<ManagementUpgradeBlockerDto> blockers,
        string currentVersion,
        string targetVersion,
        string targetApiVersion)
    {
        var currentVersionIsValid = TryParseSemanticVersion(currentVersion, out var current);
        if (!currentVersionIsValid)
        {
            AddBlocker(
                blockers,
                "current_version_unavailable",
                "The current ISLAMU runtime version is not a supported semantic version.");
        }

        if (!TryParseSemanticVersion(targetVersion, out var target))
        {
            AddBlocker(
                blockers,
                "target_version_invalid",
                "The target ISLAMU version must contain a semantic numeric core.");
        }
        else if (currentVersionIsValid && target.CompareTo(current) < 0)
        {
            AddBlocker(
                blockers,
                "target_version_older",
                "The target ISLAMU version cannot be older than the current runtime.");
        }

        AddManagementApiBlocker(blockers, targetApiVersion, "target_management_api_incompatible");
    }

    public static void AddPostflightVersionBlockers(
        ICollection<ManagementUpgradeBlockerDto> blockers,
        string currentVersion,
        string expectedVersion,
        string expectedApiVersion)
    {
        if (!VersionsMatch(currentVersion, expectedVersion))
        {
            AddBlocker(
                blockers,
                "event_version_mismatch",
                "The observed ISLAMU runtime does not match the expected upgraded version.");
        }

        AddManagementApiBlocker(blockers, expectedApiVersion, "management_api_version_mismatch");
    }

    public static void AddBlocker(
        ICollection<ManagementUpgradeBlockerDto> blockers,
        string code,
        string message) =>
        blockers.Add(new ManagementUpgradeBlockerDto(code, message));

    private static void AddManagementApiBlocker(
        ICollection<ManagementUpgradeBlockerDto> blockers,
        string candidate,
        string code)
    {
        if (!string.Equals(
                candidate,
                ManagedControlPlaneContract.ManagementApiVersion,
                StringComparison.Ordinal))
        {
            AddBlocker(
                blockers,
                code,
                "The requested management API version is not supported by this ISLAMU runtime.");
        }
    }

    private static bool VersionsMatch(string currentVersion, string expectedVersion)
    {
        if (!TryParseSemanticVersion(currentVersion, out var current)
            || !TryParseSemanticVersion(expectedVersion, out var expected))
        {
            return false;
        }

        return current.CompareTo(expected) == 0;
    }

    private static bool TryParseSemanticVersion(string value, out SemanticVersion version)
    {
        version = default;
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized) || normalized.Length > 100)
        {
            return false;
        }

        var buildParts = normalized.Split('+');
        if (buildParts.Length > 2
            || (buildParts.Length == 2 && !IdentifiersAreValid(buildParts[1], false)))
        {
            return false;
        }

        var versionAndPrerelease = buildParts[0];
        var prereleaseSeparator = versionAndPrerelease.IndexOf('-');
        var core = prereleaseSeparator < 0
            ? versionAndPrerelease
            : versionAndPrerelease[..prereleaseSeparator];
        var prerelease = prereleaseSeparator < 0
            ? []
            : versionAndPrerelease[(prereleaseSeparator + 1)..].Split('.');
        var segments = core.Split('.');
        if (segments.Length != 3
            || segments.Any(segment => !TryParseCoreIdentifier(segment, out _))
            || (prerelease.Length > 0 && !IdentifiersAreValid(prerelease, true)))
        {
            return false;
        }

        version = new SemanticVersion(
            int.Parse(segments[0], System.Globalization.CultureInfo.InvariantCulture),
            int.Parse(segments[1], System.Globalization.CultureInfo.InvariantCulture),
            int.Parse(segments[2], System.Globalization.CultureInfo.InvariantCulture),
            prerelease);
        return true;
    }

    private static bool TryParseCoreIdentifier(string value, out int parsed)
    {
        parsed = 0;
        return value.Length > 0
            && (value.Length == 1 || value[0] != '0')
            && int.TryParse(
                value,
                System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture,
                out parsed);
    }

    private static bool IdentifiersAreValid(string value, bool rejectNumericLeadingZero) =>
        IdentifiersAreValid(value.Split('.'), rejectNumericLeadingZero);

    private static bool IdentifiersAreValid(
        IReadOnlyCollection<string> identifiers,
        bool rejectNumericLeadingZero) =>
        identifiers.Count > 0
        && identifiers.All(identifier =>
            identifier.Length > 0
            && identifier.All(character =>
                character is >= '0' and <= '9'
                or >= 'A' and <= 'Z'
                or >= 'a' and <= 'z'
                or '-')
            && (!rejectNumericLeadingZero
                || !IsNumericIdentifier(identifier)
                || identifier.Length == 1
                || identifier[0] != '0'));

    private static bool IsNumericIdentifier(string value) =>
        value.All(character => character is >= '0' and <= '9');

    private readonly record struct SemanticVersion(
        int Major,
        int Minor,
        int Patch,
        IReadOnlyList<string> Prerelease) : IComparable<SemanticVersion>
    {
        public int CompareTo(SemanticVersion other)
        {
            var coreComparison = Major.CompareTo(other.Major);
            if (coreComparison == 0)
            {
                coreComparison = Minor.CompareTo(other.Minor);
            }

            if (coreComparison == 0)
            {
                coreComparison = Patch.CompareTo(other.Patch);
            }

            if (coreComparison != 0)
            {
                return coreComparison;
            }

            if (Prerelease.Count == 0 || other.Prerelease.Count == 0)
            {
                return Prerelease.Count == other.Prerelease.Count
                    ? 0
                    : Prerelease.Count == 0 ? 1 : -1;
            }

            for (var index = 0; index < Math.Min(Prerelease.Count, other.Prerelease.Count); index++)
            {
                var identifierComparison = CompareIdentifier(Prerelease[index], other.Prerelease[index]);
                if (identifierComparison != 0)
                {
                    return identifierComparison;
                }
            }

            return Prerelease.Count.CompareTo(other.Prerelease.Count);
        }

        private static int CompareIdentifier(string left, string right)
        {
            var leftIsNumeric = IsNumericIdentifier(left);
            var rightIsNumeric = IsNumericIdentifier(right);
            if (leftIsNumeric && rightIsNumeric)
            {
                var lengthComparison = left.Length.CompareTo(right.Length);
                return lengthComparison != 0
                    ? lengthComparison
                    : string.Compare(left, right, StringComparison.Ordinal);
            }

            if (leftIsNumeric != rightIsNumeric)
            {
                return leftIsNumeric ? -1 : 1;
            }

            return string.Compare(left, right, StringComparison.Ordinal);
        }
    }
}
