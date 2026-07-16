// ABOUTME: Static catalog of concrete resource descriptors for all DTO types participating in authorization.
// ABOUTME: Each descriptor extracts authorization metadata (ID, attributes, scope) from its DTO instance.

using Explore.Application.DTOs.ActorSubscription;
using Explore.Application.DTOs.Ai;
using Explore.Application.DTOs.AtprotoRecord;
using Explore.Application.DTOs.Category;
using Explore.Application.DTOs.CustomPropertyDefinition;
using Explore.Application.DTOs.EmailDispatch;
using Explore.Application.DTOs.Group;
using Explore.Application.DTOs.GroupMember;
using Explore.Application.DTOs.IndexedDid;
using Explore.Application.DTOs.Location;
using Explore.Application.DTOs.LocationRoom;
using Explore.Application.DTOs.Organization;
using Explore.Application.DTOs.OrganizationMember;
using Explore.Application.DTOs.OrganizationReview;
using Explore.Application.DTOs.StorageObject;
using Explore.Application.DTOs.SupportAccess;
using Explore.Application.DTOs.Tag;
using Explore.Application.DTOs.Tenant;
using Explore.Application.DTOs.TenantSettingsDocuments;
using Explore.Application.DTOs.TenantUserRoleGrant;
using Explore.Application.DTOs.User;
using Explore.Application.DTOs.Webhooks;

namespace Explore.Application.Authorization;

/// <summary>
/// Central catalog of resource descriptors for all DTO types participating in HATEOAS authorization.
/// <para>
/// Usage in link policies:
/// <code>
/// .RequirePermission(AuthorizationActions.Update, ResourceDescriptors.Organization, dto)
/// </code>
/// replaces manual dictionary construction with type-safe, centralized metadata extraction.
/// </para>
/// <para>
/// Descriptors marked "piggybacks on tenant" use <see cref="ResourceKinds.Tenant"/> because
/// their commands authorize as the parent tenant resource, not as their own resource kind.
/// </para>
/// </summary>
public static class ResourceDescriptors
{
    #region Core resources with unique Cerbos resource kinds

    public static readonly ResourceDescriptor<OrganizationDto> Organization = new(
        ResourceKinds.Organization,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["organizationId"] = dto.Id.ToString(),
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    /// <summary>List DTO variant for collection item-level permission checks.</summary>
    public static readonly ResourceDescriptor<OrganizationListDto> OrganizationList = new(
        ResourceKinds.Organization,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["organizationId"] = dto.Id.ToString(),
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<TenantDto> Tenant = new(
        ResourceKinds.Tenant,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.Id.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.Id.ToString()));

    public static readonly ResourceDescriptor<TenantBrandingSettingsDocumentDto> TenantBrandingSettingsDocument = new(
        ResourceKinds.TenantSetting,
        dto => $"{dto.SourceScopeId}:{dto.DocumentKey}",
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.SourceScopeId.ToString(),
            ["documentKey"] = dto.DocumentKey,
            ["isLockedByInstance"] = dto.IsLockedByInstance
        },
        dto => new AuthorizationScope(TenantId: dto.SourceScopeId.ToString()));

    public static readonly ResourceDescriptor<TenantUserRoleGrantDto> TenantUserRoleGrant = new(
        ResourceKinds.TenantUserRoleGrant,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["tenantUserId"] = dto.TenantUserId.ToString(),
            ["userId"] = dto.UserId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<SupportAccessSessionDto> SupportAccessSession = new(
        ResourceKinds.SupportAccessSession,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["sessionId"] = dto.Id.ToString(),
            ["tenantId"] = dto.TargetTenantId.ToString(),
            ["actorUserId"] = dto.ActorUserId.ToString(),
            ["mode"] = dto.ModeName,
            ["status"] = dto.StatusName
        },
        dto => new AuthorizationScope(TenantId: dto.TargetTenantId.ToString()));

    public static readonly ResourceDescriptor<SupportAccessAuditEventDto> SupportAccessAuditEvent = new(
        ResourceKinds.SupportAccessSession,
        dto => dto.SupportAccessSessionId.ToString(),
        dto => new Dictionary<string, object>
        {
            ["sessionId"] = dto.SupportAccessSessionId.ToString(),
            ["tenantId"] = dto.TargetTenantId.ToString(),
            ["auditEventId"] = dto.Id.ToString(),
            ["eventType"] = dto.EventTypeName
        },
        dto => new AuthorizationScope(TenantId: dto.TargetTenantId.ToString()));

    public static readonly ResourceDescriptor<WebhookConsumerDto> WebhookConsumer = new(
        ResourceKinds.Webhook,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["consumerId"] = dto.Id.ToString(),
            ["consumerKind"] = dto.ConsumerKindName,
            ["providerMode"] = dto.ProviderModeName,
            ["status"] = dto.StatusName
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<WebhookEndpointDto> WebhookEndpoint = new(
        ResourceKinds.Webhook,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["consumerId"] = dto.ConsumerId.ToString(),
            ["endpointId"] = dto.Id.ToString(),
            ["status"] = dto.StatusName
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<WebhookMessageDto> WebhookMessage = new(
        ResourceKinds.Webhook,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["messageId"] = dto.Id.ToString(),
            ["eventType"] = dto.EventType,
            ["aggregateKind"] = dto.AggregateKind
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<WebhookDeliveryAttemptDto> WebhookDeliveryAttempt = new(
        ResourceKinds.Webhook,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["attemptId"] = dto.Id.ToString(),
            ["messageId"] = dto.MessageId.ToString(),
            ["endpointId"] = dto.EndpointId.ToString(),
            ["outcome"] = dto.OutcomeCode
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<ActorSubscriptionDto> ActorSubscription = new(
        ResourceKinds.ActorSubscription,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["subscriberTenantUserId"] = dto.SubscriberTenantUserId.ToString(),
            ["subscriberUserId"] = dto.SubscriberUserId.ToString(),
            ["targetActorId"] = dto.TargetActorId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<ActorSubscriptionListDto> ActorSubscriptionList = new(
        ResourceKinds.ActorSubscription,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["targetActorId"] = dto.TargetActorId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<AiConversationDto> AiConversation = new(
        ResourceKinds.AiConversation,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["userId"] = dto.UserId.ToString(),
            ["status"] = dto.Status
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<AiConversationSummaryDto> AiConversationSummary = new(
        ResourceKinds.AiConversation,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["userId"] = dto.UserId.ToString(),
            ["status"] = dto.Status
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<LocationRoomDto> LocationRoom = new(
        ResourceKinds.LocationRoom,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["locationId"] = dto.LocationId.ToString(),
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<CategoryDto> Category = new(
        ResourceKinds.Category,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<TagDto> Tag = new(
        ResourceKinds.Tag,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<LocationDto> Location = new(
        ResourceKinds.Location,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<StorageObjectDto> StorageObject = new(
        ResourceKinds.StorageObject,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["visibility"] = dto.Visibility,
            ["lifecycleState"] = dto.LifecycleState
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<OrganizationMemberDto> OrganizationMember = new(
        ResourceKinds.OrganizationMember,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["organizationId"] = dto.OrganizationId.ToString(),
            ["userId"] = dto.UserId.ToString()
        },
        dto => new AuthorizationScope(
            TenantId: dto.TenantId.ToString(),
            OrganizationId: dto.OrganizationId.ToString()));

    public static readonly ResourceDescriptor<OrganizationReviewDto> OrganizationReview = new(
        ResourceKinds.OrganizationReview,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["organizationId"] = dto.OrganizationId.ToString(),
            ["userId"] = dto.UserId.ToString()
        },
        dto => new AuthorizationScope(OrganizationId: dto.OrganizationId.ToString()));

    public static readonly ResourceDescriptor<GroupDto> Group = new(
        ResourceKinds.Group,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    /// <summary>List DTO variant for collection item-level permission checks.</summary>
    public static readonly ResourceDescriptor<GroupListDto> GroupList = new(
        ResourceKinds.Group,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["groupId"] = dto.Id.ToString(),
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<GroupMemberDto> GroupMember = new(
        ResourceKinds.GroupMember,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["groupId"] = dto.GroupId.ToString(),
            ["userId"] = dto.UserId.ToString()
        });

    public static readonly ResourceDescriptor<UserDto> User = new(
        ResourceKinds.User,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["actorId"] = dto.ActorId.ToString()
        });

    public static readonly ResourceDescriptor<CustomPropertyDefinitionDto> CustomPropertyDefinition = new(
        ResourceKinds.CustomPropertyDefinition,
        dto => dto.Id.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString()
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<EmailDispatchStatusDto> EmailDispatchStatus = new(
        ResourceKinds.EmailDispatch,
        dto => dto.OutboxId.ToString(),
        dto => new Dictionary<string, object>
        {
            ["tenantId"] = dto.TenantId.ToString(),
            ["outboxId"] = dto.OutboxId.ToString(),
            ["sourceType"] = dto.SourceType,
            ["sourceId"] = dto.SourceId.ToString(),
            ["deliveryStatus"] = dto.DeliveryStatus
        },
        dto => new AuthorizationScope(TenantId: dto.TenantId.ToString()));

    public static readonly ResourceDescriptor<IndexedDidDto> IndexedDid = new(
        ResourceKinds.IndexedDid,
        dto => dto.Did);

    public static readonly ResourceDescriptor<AtprotoRecordDto> AtprotoRecord = new(
        ResourceKinds.AtprotoRecord,
        dto => dto.Id.ToString());

    #endregion

}
