// ABOUTME: Batch evaluation optimization for FallbackAuthorizationService.
// ABOUTME: Pre-resolves admin authority once per batch to eliminate repeated async overhead.

using Explore.Application.Authorization;
using Explore.Application.Contracts.Infrastructure;
using Explore.Domain;

namespace Explore.Infrastructure.Services;

public partial class FallbackAuthorizationService
{
    public async Task<IReadOnlyList<bool>> IsAllowedBatchAsync(
        IReadOnlyList<AuthorizationCheck> checks,
        CancellationToken cancellationToken = default)
    {
        if (checks.Count == 0)
            return [];

        if (checks.Count <= 2)
        {
            var smallResults = new bool[checks.Count];
            for (var i = 0; i < checks.Count; i++)
            {
                var check = checks[i];
                smallResults[i] = await IsAllowedAsync(
                    check.ResourceKind,
                    check.ResourceId,
                    check.Action,
                    check.ResourceAttributes is null ? null : new Dictionary<string, object>(check.ResourceAttributes),
                    cancellationToken);
            }

            return smallResults;
        }

        var profile = await ResolveAuthorityProfileAsync(cancellationToken);

        var results = new bool[checks.Count];
        for (var i = 0; i < checks.Count; i++)
        {
            var check = checks[i];
            var attributes = check.ResourceAttributes is null
                ? null
                : new Dictionary<string, object>(check.ResourceAttributes);
            results[i] = EvaluateWithProfile(profile, check.ResourceKind, check.ResourceId, check.Action, attributes);
        }

        return results;
    }

    private sealed record AuthorityProfile(
        bool IsInstanceAdmin,
        bool IsTenantAdmin,
        Guid TenantId,
        IReadOnlySet<Guid> AdminOrgIds,
        Guid? UserId);

    private async Task<AuthorityProfile> ResolveAuthorityProfileAsync(CancellationToken cancellationToken)
    {
        var isInstanceAdmin = await _adminContext.IsInstanceAdminAsync(cancellationToken);
        var tenantId = _tenantContext.TenantId;
        var isTenantAdmin = !isInstanceAdmin && await _adminContext.IsTenantAdminAsync(tenantId, cancellationToken);
        var adminOrgIds = (await _adminContext.GetAdminOrganizationIdsAsync(cancellationToken) ?? []).ToHashSet();

        var userId = _adminContext.UserId ?? await _adminContext.ResolveUserIdAsync(cancellationToken);
        return new AuthorityProfile(isInstanceAdmin, isTenantAdmin, tenantId, adminOrgIds, userId);
    }

    private bool EvaluateWithProfile(
        AuthorityProfile profile,
        string resourceKind,
        string resourceId,
        string action,
        IDictionary<string, object>? resourceAttributes)
    {
        if (profile.IsInstanceAdmin)
        {
            LogDecision("allow", "is_instance_admin", resourceKind, resourceId, action);
            return true;
        }

        if (SafeMode && !profile.IsInstanceAdmin)
        {
            LogDecision("deny", "safe_mode_active", resourceKind, resourceId, action);
            return false;
        }

        var decision = resourceKind switch
        {
            "islamuevent_instance_setting" => false,
            "islamuevent_tenant_setting" => EvaluateTenantSettingWithProfile(profile, resourceId, action, resourceAttributes),
            "islamuevent_tenant" => action is AuthorizationActions.View or AuthorizationActions.Update
                && EvaluateTenantScopedWithProfile(profile, resourceAttributes),
            "islamuevent_tenant_user_role_grant" or "islamuevent_category" or "islamuevent_tag" or "islamuevent_location" or "islamuevent_location_room"
                => EvaluateTenantScopedWithProfile(profile, resourceAttributes),
            "islamuevent_custom_property_definition" or "islamuevent_custom_property_template" or "islamuevent_actor"
                => action is "view" || EvaluateTenantScopedWithProfile(profile, resourceAttributes),
            "islamuevent_custom_property_value"
                => action is "view" || IsAdminForOrgScope(profile, resourceAttributes, resourceId),
            "islamuevent_custom_property_projection"
                => EvaluateCustomPropertyProjectionWithProfile(profile, action, resourceAttributes),
            "islamuevent_custom_property_governance" or "islamuevent_email_dispatch" or "islamuevent_webhook"
                => EvaluateTenantScopedWithProfile(profile, resourceAttributes),
            "islamuevent_support_access_session"
                => EvaluateSupportAccessSessionWithProfile(profile, action, resourceAttributes),
            "islamuevent_platform_namespace" => action is "view",
            "islamuevent_organization" when action is "create" && HasAuthorizationPhase(resourceAttributes, AuthorizationPhases.PreCreate)
                => IsOrganizationCreateAllowedForProfile(profile, resourceAttributes),
            "islamuevent_organization" => profile.IsTenantAdmin || IsOrgAdminFromProfile(profile, resourceAttributes, resourceId),
            "islamuevent_organization_member" => IsAdminForOrgScope(profile, resourceAttributes, resourceId),
            "islamuevent_organization_review" => action is "create" or "view" || IsAdminForOrgScope(profile, resourceAttributes, resourceId),
            "islamuevent_group" => action is "view" || IsAdminForOrgScope(profile, resourceAttributes, resourceId),
            "islamuevent_group_member" => action is "view" or "create" || IsAdminForOrgScope(profile, resourceAttributes, resourceId),
            "islamuevent_storage_object" => EvaluateStorageObjectWithProfile(profile, resourceId, action, resourceAttributes),
            "islamuevent_user" => EvaluateUserWithProfile(profile, resourceId, action),
            "islamuevent_notification" => true,
            "islamuevent_actor_subscription" => true,
            "islamuevent_ai_conversation" => true,
            "islamuevent_atproto_record" or "islamuevent_indexed_did" => false,
            _ => false
        };

        LogDecision(decision ? "allow" : "deny", "fallback_batch_policy", resourceKind, resourceId, action);
        return decision;
    }

    private static bool EvaluateTenantSettingWithProfile(
        AuthorityProfile profile,
        string resourceId,
        string action,
        IDictionary<string, object>? resourceAttributes)
    {
        if (!IsTenantBrandingDocument(resourceAttributes)
            && resourceAttributes?.TryGetValue("isLockedByInstance", out var lockedObj) == true
            && lockedObj is true)
        {
            return false;
        }

        return profile.IsTenantAdmin;
    }

    private static bool EvaluateUserWithProfile(AuthorityProfile profile, string resourceId, string action)
    {
        if (action is "view" or "update" && profile.UserId.HasValue
            && Guid.TryParse(resourceId, out var targetUserId)
            && targetUserId == profile.UserId.Value)
            return true;

        return profile.IsTenantAdmin;
    }

    private static bool EvaluateTenantScopedWithProfile(
        AuthorityProfile profile,
        IDictionary<string, object>? resourceAttributes)
    {
        if (resourceAttributes?.ContainsKey("tenantId") != true)
            return profile.IsTenantAdmin;

        return profile.IsTenantAdmin
            && TryResolveGuidAttribute(resourceAttributes, "tenantId", out var tenantId)
            && tenantId == profile.TenantId;
    }

    private static bool EvaluateCustomPropertyProjectionWithProfile(
        AuthorityProfile profile,
        string action,
        IDictionary<string, object>? resourceAttributes)
    {
        return action is AuthorizationActions.CustomPropertyProjections.View
                or AuthorizationActions.CustomPropertyProjections.Update
            && profile.IsTenantAdmin
            && TryResolveGuidAttribute(resourceAttributes, "tenantId", out var tenantId)
            && tenantId == profile.TenantId;
    }

    private static bool EvaluateSupportAccessSessionWithProfile(
        AuthorityProfile profile,
        string action,
        IDictionary<string, object>? resourceAttributes)
    {
        return action is AuthorizationActions.SupportAccessSessions.View
                or AuthorizationActions.SupportAccessSessions.List
                or AuthorizationActions.SupportAccessSessions.ViewAudit
            && profile.IsTenantAdmin
            && TryResolveGuidAttribute(resourceAttributes, "tenantId", out var tenantId)
            && tenantId == profile.TenantId;
    }

    private static bool EvaluateStorageObjectWithProfile(
        AuthorityProfile profile,
        string resourceId,
        string action,
        IDictionary<string, object>? resourceAttributes)
    {
        if (action == AuthorizationActions.StorageObjects.Create)
            return true;

        if (action is AuthorizationActions.StorageObjects.Download
            or AuthorizationActions.StorageObjects.PresignedDownload)
        {
            return EvaluateTenantScopedWithProfile(profile, resourceAttributes)
                || CanReadStorageObjectContentWithProfile(profile, resourceId, resourceAttributes);
        }

        return EvaluateTenantScopedWithProfile(profile, resourceAttributes);
    }

    private static bool CanReadStorageObjectContentWithProfile(
        AuthorityProfile profile,
        string resourceId,
        IDictionary<string, object>? resourceAttributes)
    {
        var visibility = GetAttribute(resourceAttributes, "visibility");
        var lifecycleState = GetAttribute(resourceAttributes, "lifecycleState");
        if (!string.Equals(lifecycleState, StorageObjectLifecycleStates.Active, StringComparison.Ordinal))
            return false;

        if (visibility is StorageObjectVisibilities.PublicImage or StorageObjectVisibilities.AuthenticatedTenant)
            return true;

        var createdBy = GetAttribute(resourceAttributes, "createdBy");
        var currentUserId = profile.UserId?.ToString("D");
        return !string.IsNullOrWhiteSpace(createdBy)
            && !string.IsNullOrWhiteSpace(currentUserId)
            && string.Equals(createdBy, currentUserId, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsOrganizationCreateAllowedForProfile(
        AuthorityProfile profile,
        IDictionary<string, object>? resourceAttributes)
    {
        var tenantId = ResolveTenantId(resourceAttributes);
        return tenantId == profile.TenantId && profile.UserId.HasValue;
    }

    private static bool HasAuthorizationPhase(
        IDictionary<string, object>? resourceAttributes,
        string phase)
    {
        return resourceAttributes?.TryGetValue("authorizationPhase", out var value) == true
            && string.Equals(value?.ToString(), phase, StringComparison.Ordinal);
    }

    private static bool IsOrgAdminFromProfile(
        AuthorityProfile profile,
        IDictionary<string, object>? resourceAttributes,
        string resourceId)
    {
        var orgId = ResolveOrganizationId(resourceAttributes, resourceId);
        return orgId.HasValue && profile.AdminOrgIds.Contains(orgId.Value);
    }

    private static bool IsAdminForOrgScope(
        AuthorityProfile profile,
        IDictionary<string, object>? resourceAttributes,
        string resourceId)
    {
        if (profile.IsTenantAdmin)
            return true;

        return IsOrgAdminFromProfile(profile, resourceAttributes, resourceId);
    }

}
