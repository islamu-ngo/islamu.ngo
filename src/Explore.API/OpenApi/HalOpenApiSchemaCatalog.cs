// ABOUTME: Explicit API-owned catalog of DTOs whose HAL OpenAPI schemas are public contract surface.
// ABOUTME: Keeps HAL schema opt-in out of Application DTOs and avoids broad namespace reflection.

namespace Explore.API.OpenApi;

internal static class HalOpenApiSchemaCatalog
{
    public static IReadOnlyList<Type> RegisteredDtoTypes { get; } =
    [
        // Category DTOs
        typeof(Explore.Application.DTOs.Category.CategoryDto),
        typeof(Explore.Application.DTOs.Category.CategoryListDto),

        // Tag DTOs
        typeof(Explore.Application.DTOs.Tag.TagDto),
        typeof(Explore.Application.DTOs.Tag.TagListDto),

        // Location DTOs
        typeof(Explore.Application.DTOs.Location.LocationDto),
        typeof(Explore.Application.DTOs.Location.LocationListDto),

        // Organization DTOs
        typeof(Explore.Application.DTOs.Organization.OrganizationDto),
        typeof(Explore.Application.DTOs.Organization.OrganizationListDto),

        // Actor DTOs
        typeof(Explore.Application.DTOs.Actor.ActorDto),
        typeof(Explore.Application.DTOs.Actor.ActorListDto),
        typeof(Explore.Application.DTOs.ActorSubscription.ActorSubscriptionDto),
        typeof(Explore.Application.DTOs.ActorSubscription.ActorSubscriptionListDto),

        // Public metadata DTOs
        typeof(Explore.Application.DTOs.CustomPropertyDefinition.CustomPropertyDefinitionDto),
        typeof(Explore.Application.DTOs.CustomPropertyDefinition.CustomPropertyDefinitionListDto),
        typeof(Explore.Application.DTOs.Group.GroupDto),
        typeof(Explore.Application.DTOs.Group.GroupListDto),
        typeof(Explore.Application.DTOs.GroupMember.GroupMemberDto),
        typeof(Explore.Application.DTOs.OrganizationMember.OrganizationMemberDto),
        typeof(Explore.Application.DTOs.IndexedDid.IndexedDidDto),
        typeof(Explore.Application.DTOs.IndexedDid.IndexedDidListDto),
        typeof(Explore.Application.DTOs.LocationRoom.LocationRoomDto),
        typeof(Explore.Application.DTOs.LocationRoom.LocationRoomListDto),
        typeof(Explore.Application.DTOs.OrganizationReview.OrganizationReviewDto),
        typeof(Explore.Application.DTOs.TenantUserRoleGrant.TenantUserRoleGrantDto),
        typeof(Explore.Application.DTOs.TenantUserRoleGrant.TenantUserRoleGrantListDto),
        typeof(Explore.Application.DTOs.Webhooks.WebhookConsumerDto),
        typeof(Explore.Application.DTOs.Webhooks.WebhookEndpointDto),
        typeof(Explore.Application.DTOs.Webhooks.WebhookMessageDto),
        typeof(Explore.Application.DTOs.Webhooks.WebhookDeliveryAttemptDto),

        // Email dispatch admin DTOs
        typeof(Explore.Application.DTOs.EmailDispatch.EmailDispatchStatusDto),

        typeof(Explore.Application.DTOs.SupportAccess.SupportAccessSessionDto),
        typeof(Explore.Application.DTOs.SupportAccess.SupportAccessAuditEventDto),

        // Tenant settings DTOs
        typeof(Explore.Application.DTOs.TenantSettingsDocuments.TenantBrandingSettingsDocumentDto),

        // AI assistant DTOs
        typeof(Explore.Application.DTOs.Ai.AiAssistantBootstrapDto),
        typeof(Explore.Application.DTOs.Ai.AiConversationSummaryDto),
        typeof(Explore.Application.DTOs.Ai.AiConversationDto),
        typeof(Explore.Application.DTOs.Ai.AiRunDto),

        // Storage DTOs
        typeof(Explore.Application.DTOs.StorageObject.StorageObjectDto),
        typeof(Explore.Application.DTOs.StorageObject.StorageObjectListDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOverviewDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDomainOverviewDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDnsRecordDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOperationsDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOperationStatusDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOperationMetricDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDeploymentModeRunbookDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDeploymentModeTargetOptionDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDeploymentModeRunbookStepDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantDetailDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantListItemDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanDetailDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanListItemDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanVersionDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanSettingDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanQuotaDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanAssignmentDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantEffectiveConfigurationDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantEffectiveSettingDto),
        typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantQuotaUsageDto),
        typeof(Explore.Application.DTOs.Onboarding.InstanceOnboardingStatusDto),
        typeof(Explore.Application.DTOs.Onboarding.TenantOnboardingStatusDto),
        typeof(Explore.Application.DTOs.Onboarding.InstanceStorageSettingsDto),
        typeof(Explore.Application.DTOs.Notification.NotificationPreferenceMatrixDto),
        typeof(Explore.Application.DTOs.Notification.WebPushSubscriptionDto),
        typeof(Explore.Application.DTOs.Tenant.TenantStorageSettingsDto),
    ];

    public static IReadOnlyDictionary<string, Type> DetailResourceMappings { get; } = new Dictionary<string, Type>
    {
        ["HalResourceOfCategoryDto"] = typeof(Explore.Application.DTOs.Category.CategoryDto),
        ["HalResourceOfCategoryListDto"] = typeof(Explore.Application.DTOs.Category.CategoryListDto),
        ["HalResourceOfTagDto"] = typeof(Explore.Application.DTOs.Tag.TagDto),
        ["HalResourceOfTagListDto"] = typeof(Explore.Application.DTOs.Tag.TagListDto),
        ["HalResourceOfLocationDto"] = typeof(Explore.Application.DTOs.Location.LocationDto),
        ["HalResourceOfLocationListDto"] = typeof(Explore.Application.DTOs.Location.LocationListDto),
        ["HalResourceOfOrganizationDto"] = typeof(Explore.Application.DTOs.Organization.OrganizationDto),
        ["HalResourceOfOrganizationListDto"] = typeof(Explore.Application.DTOs.Organization.OrganizationListDto),
        ["HalResourceOfActorDto"] = typeof(Explore.Application.DTOs.Actor.ActorDto),
        ["HalResourceOfActorListDto"] = typeof(Explore.Application.DTOs.Actor.ActorListDto),
        ["HalResourceOfActorSubscriptionDto"] = typeof(Explore.Application.DTOs.ActorSubscription.ActorSubscriptionDto),
        ["HalResourceOfActorSubscriptionListDto"] = typeof(Explore.Application.DTOs.ActorSubscription.ActorSubscriptionListDto),
        ["HalResourceOfCustomPropertyDefinitionDto"] = typeof(Explore.Application.DTOs.CustomPropertyDefinition.CustomPropertyDefinitionDto),
        ["HalResourceOfCustomPropertyDefinitionListDto"] = typeof(Explore.Application.DTOs.CustomPropertyDefinition.CustomPropertyDefinitionListDto),
        ["HalResourceOfGroupDto"] = typeof(Explore.Application.DTOs.Group.GroupDto),
        ["HalResourceOfGroupListDto"] = typeof(Explore.Application.DTOs.Group.GroupListDto),
        ["HalResourceOfGroupMemberDto"] = typeof(Explore.Application.DTOs.GroupMember.GroupMemberDto),
        ["HalResourceOfOrganizationMemberDto"] = typeof(Explore.Application.DTOs.OrganizationMember.OrganizationMemberDto),
        ["HalResourceOfIndexedDidDto"] = typeof(Explore.Application.DTOs.IndexedDid.IndexedDidDto),
        ["HalResourceOfIndexedDidListDto"] = typeof(Explore.Application.DTOs.IndexedDid.IndexedDidListDto),
        ["HalResourceOfLocationRoomDto"] = typeof(Explore.Application.DTOs.LocationRoom.LocationRoomDto),
        ["HalResourceOfLocationRoomListDto"] = typeof(Explore.Application.DTOs.LocationRoom.LocationRoomListDto),
        ["HalResourceOfOrganizationReviewDto"] = typeof(Explore.Application.DTOs.OrganizationReview.OrganizationReviewDto),
        ["HalResourceOfTenantUserRoleGrantDto"] = typeof(Explore.Application.DTOs.TenantUserRoleGrant.TenantUserRoleGrantDto),
        ["HalResourceOfTenantUserRoleGrantListDto"] = typeof(Explore.Application.DTOs.TenantUserRoleGrant.TenantUserRoleGrantListDto),
        ["HalResourceOfWebhookConsumerDto"] = typeof(Explore.Application.DTOs.Webhooks.WebhookConsumerDto),
        ["HalResourceOfWebhookEndpointDto"] = typeof(Explore.Application.DTOs.Webhooks.WebhookEndpointDto),
        ["HalResourceOfWebhookMessageDto"] = typeof(Explore.Application.DTOs.Webhooks.WebhookMessageDto),
        ["HalResourceOfWebhookDeliveryAttemptDto"] = typeof(Explore.Application.DTOs.Webhooks.WebhookDeliveryAttemptDto),
        ["HalResourceOfEmailDispatchStatusDto"] = typeof(Explore.Application.DTOs.EmailDispatch.EmailDispatchStatusDto),
        ["HalResourceOfSupportAccessSessionDto"] = typeof(Explore.Application.DTOs.SupportAccess.SupportAccessSessionDto),
        ["HalResourceOfSupportAccessAuditEventDto"] = typeof(Explore.Application.DTOs.SupportAccess.SupportAccessAuditEventDto),
        ["HalResourceOfTenantBrandingSettingsDocumentDto"] = typeof(Explore.Application.DTOs.TenantSettingsDocuments.TenantBrandingSettingsDocumentDto),
        ["HalResourceOfNotificationPreferenceMatrixDto"] = typeof(Explore.Application.DTOs.Notification.NotificationPreferenceMatrixDto),
        ["HalResourceOfWebPushSubscriptionDto"] = typeof(Explore.Application.DTOs.Notification.WebPushSubscriptionDto),
        ["HalResourceOfAiAssistantBootstrapDto"] = typeof(Explore.Application.DTOs.Ai.AiAssistantBootstrapDto),
        ["HalResourceOfAiConversationSummaryDto"] = typeof(Explore.Application.DTOs.Ai.AiConversationSummaryDto),
        ["HalResourceOfAiConversationDto"] = typeof(Explore.Application.DTOs.Ai.AiConversationDto),
        ["HalResourceOfAiReferenceSearchResultDto"] = typeof(Explore.Application.DTOs.Ai.AiReferenceSearchResultDto),
        ["HalResourceOfAiRunDto"] = typeof(Explore.Application.DTOs.Ai.AiRunDto),
        ["HalResourceOfStorageObjectDto"] = typeof(Explore.Application.DTOs.StorageObject.StorageObjectDto),
        ["HalResourceOfStorageObjectListDto"] = typeof(Explore.Application.DTOs.StorageObject.StorageObjectListDto),
        ["HalResourceOfControlPlaneOverviewDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOverviewDto),
        ["HalResourceOfControlPlaneDomainOverviewDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDomainOverviewDto),
        ["HalResourceOfControlPlaneOperationsDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneOperationsDto),
        ["HalResourceOfControlPlaneDeploymentModeRunbookDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneDeploymentModeRunbookDto),
        ["HalResourceOfControlPlaneTenantDetailDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantDetailDto),
        ["HalResourceOfControlPlaneTenantListItemDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantListItemDto),
        ["HalResourceOfControlPlaneTenantPlanDetailDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanDetailDto),
        ["HalResourceOfControlPlaneTenantPlanListItemDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantPlanListItemDto),
        ["HalResourceOfControlPlaneTenantEffectiveConfigurationDto"] = typeof(Explore.Application.DTOs.ControlPlane.ControlPlaneTenantEffectiveConfigurationDto),
        ["HalResourceOfInstanceOnboardingStatusDto"] = typeof(Explore.Application.DTOs.Onboarding.InstanceOnboardingStatusDto),
        ["HalResourceOfTenantOnboardingStatusDto"] = typeof(Explore.Application.DTOs.Onboarding.TenantOnboardingStatusDto),
        ["HalResourceOfInstanceStorageSettingsDto"] = typeof(Explore.Application.DTOs.Onboarding.InstanceStorageSettingsDto),
        ["HalResourceOfTenantStorageSettingsDto"] = typeof(Explore.Application.DTOs.Tenant.TenantStorageSettingsDto),
    };

    public static IReadOnlyDictionary<string, string> CollectionEmbeddedItemResourceMappings { get; } = new Dictionary<string, string>
    {
        ["HalCollectionEmbeddedOfActorListDto"] = "HalResourceOfActorListDto",
        ["HalCollectionEmbeddedOfActorSubscriptionListDto"] = "HalResourceOfActorSubscriptionListDto",
        ["HalCollectionEmbeddedOfCategoryListDto"] = "HalResourceOfCategoryListDto",
        ["HalCollectionEmbeddedOfCustomPropertyDefinitionListDto"] = "HalResourceOfCustomPropertyDefinitionListDto",
        ["HalCollectionEmbeddedOfGroupListDto"] = "HalResourceOfGroupListDto",
        ["HalCollectionEmbeddedOfGroupMemberDto"] = "HalResourceOfGroupMemberDto",
        ["HalCollectionEmbeddedOfOrganizationMemberDto"] = "HalResourceOfOrganizationMemberDto",
        ["HalCollectionEmbeddedOfIndexedDidListDto"] = "HalResourceOfIndexedDidListDto",
        ["HalCollectionEmbeddedOfLocationListDto"] = "HalResourceOfLocationListDto",
        ["HalCollectionEmbeddedOfLocationRoomListDto"] = "HalResourceOfLocationRoomListDto",
        ["HalCollectionEmbeddedOfOrganizationListDto"] = "HalResourceOfOrganizationListDto",
        ["HalCollectionEmbeddedOfOrganizationReviewDto"] = "HalResourceOfOrganizationReviewDto",
        ["HalCollectionEmbeddedOfEmailDispatchStatusDto"] = "HalResourceOfEmailDispatchStatusDto",
        ["HalCollectionEmbeddedOfSupportAccessSessionDto"] = "HalResourceOfSupportAccessSessionDto",
        ["HalCollectionEmbeddedOfSupportAccessAuditEventDto"] = "HalResourceOfSupportAccessAuditEventDto",
        ["HalCollectionEmbeddedOfTagListDto"] = "HalResourceOfTagListDto",
        ["HalCollectionEmbeddedOfTenantUserRoleGrantListDto"] = "HalResourceOfTenantUserRoleGrantListDto",
        ["HalCollectionEmbeddedOfWebhookConsumerDto"] = "HalResourceOfWebhookConsumerDto",
        ["HalCollectionEmbeddedOfWebhookEndpointDto"] = "HalResourceOfWebhookEndpointDto",
        ["HalCollectionEmbeddedOfWebhookMessageDto"] = "HalResourceOfWebhookMessageDto",
        ["HalCollectionEmbeddedOfWebhookDeliveryAttemptDto"] = "HalResourceOfWebhookDeliveryAttemptDto",
        ["HalCollectionEmbeddedOfAiConversationSummaryDto"] = "HalResourceOfAiConversationSummaryDto",
        ["HalCollectionEmbeddedOfAiReferenceSearchResultDto"] = "HalResourceOfAiReferenceSearchResultDto",
        ["HalCollectionEmbeddedOfStorageObjectListDto"] = "HalResourceOfStorageObjectListDto",
        ["HalCollectionEmbeddedOfControlPlaneTenantListItemDto"] = "HalResourceOfControlPlaneTenantListItemDto",
        ["HalCollectionEmbeddedOfControlPlaneTenantPlanListItemDto"] = "HalResourceOfControlPlaneTenantPlanListItemDto",
    };

    public static bool IsCatalogedDetailResourceSchema(string schemaName)
        => DetailResourceMappings.ContainsKey(schemaName);

    public static bool IsRegisteredDto(Type dtoType)
        => RegisteredDtoTypes.Contains(dtoType);
}
