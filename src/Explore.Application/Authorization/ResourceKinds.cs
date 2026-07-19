// ABOUTME: Canonical catalog of resource kind string constants matching Cerbos policy resource names.
// ABOUTME: Replaces magic strings in [AuthorizeResource] attributes and ResourceDescriptorRegistry lookups.

namespace Explore.Application.Authorization;

/// <summary>
/// Canonical catalog of resource kind identifiers used for authorization.
/// All values match Cerbos resource policy names exactly.
/// <para>
/// Use these constants in <see cref="AuthorizeResourceAttribute"/>, link policies,
/// and anywhere a resource kind string is required. Avoid raw string literals.
/// </para>
/// </summary>
public static class ResourceKinds
{
    public const string Organization = "islamuevent_organization";
    public const string OrganizationMember = "islamuevent_organization_member";
    public const string OrganizationReview = "islamuevent_organization_review";
    public const string Tenant = "islamuevent_tenant";
    public const string TenantSetting = "islamuevent_tenant_setting";
    public const string TenantUserRoleGrant = "islamuevent_tenant_user_role_grant";
    public const string SupportAccessSession = "islamuevent_support_access_session";
    public const string Category = "islamuevent_category";
    public const string Tag = "islamuevent_tag";
    public const string Location = "islamuevent_location";
    public const string LocationRoom = "islamuevent_location_room";
    public const string StorageObject = "islamuevent_storage_object";
    public const string User = "islamuevent_user";
    public const string AtprotoRecord = "islamuevent_atproto_record";
    public const string IndexedDid = "islamuevent_indexed_did";
    public const string InstanceSetting = "islamuevent_instance_setting";
    public const string CustomPropertyDefinition = "islamuevent_custom_property_definition";
    public const string CustomPropertyTemplate = "islamuevent_custom_property_template";
    public const string CustomPropertyValue = "islamuevent_custom_property_value";
    public const string CustomPropertyGovernance = "islamuevent_custom_property_governance";
    public const string EmailDispatch = "islamuevent_email_dispatch";
    public const string Blog = "islamuevent_blog";
    public const string Webhook = "islamuevent_webhook";
    public const string PlatformNamespace = "islamuevent_platform_namespace";
    public const string Notification = "islamuevent_notification";
    public const string Actor = "islamuevent_actor";
    public const string ActorSubscription = "islamuevent_actor_subscription";
    public const string AiConversation = "islamuevent_ai_conversation";
    public const string Group = "islamuevent_group";
    public const string GroupMember = "islamuevent_group_member";
}
