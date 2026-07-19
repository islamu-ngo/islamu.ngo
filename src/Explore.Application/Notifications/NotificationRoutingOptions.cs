// ABOUTME: Configuration model for notification ownership routing defaults.
// ABOUTME: Validates account-authority and external-delegation rules before resolver decisions are used.

namespace Explore.Application.Notifications;

public sealed class NotificationRoutingOptions
{
    public const string SectionName = "NotificationRouting";

    public NotificationOwnership IdentityLifecycleOwner { get; set; } = NotificationOwnership.AccountAuthority;
    public AccountAuthorityKind DefaultAccountAuthorityKind { get; set; } = AccountAuthorityKind.Keycloak;

    public NotificationOwnership ProductLifecycleOwner { get; set; } = NotificationOwnership.Disabled;
    public NotificationOwnership ProviderInternalOwner { get; set; } = NotificationOwnership.ExternalWorkflowProvider;
    public NotificationOwnership PlatformOperationsOwner { get; set; } = NotificationOwnership.Disabled;
    public NotificationOwnership MarketingOwner { get; set; } = NotificationOwnership.Disabled;

    public ExternalWorkflowProviderKind ProviderInternalProvider { get; set; } = ExternalWorkflowProviderKind.Other;

    public NotificationOwnership GetOwner(NotificationCategory category) => category switch
    {
        NotificationCategory.IdentityLifecycle => IdentityLifecycleOwner,
        NotificationCategory.ProductLifecycle => ProductLifecycleOwner,
        NotificationCategory.ProviderInternal => ProviderInternalOwner,
        NotificationCategory.PlatformOperations => PlatformOperationsOwner,
        NotificationCategory.Marketing => MarketingOwner,
        _ => NotificationOwnership.Disabled
    };

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        ValidateDefinedOwners(errors);
        ValidateAccountAuthority(errors);
        ValidateDisabledCategories(errors);
        ValidateExternalProviderRules(errors);

        return errors;
    }

    private void ValidateDefinedOwners(List<string> errors)
    {
        foreach (var (name, owner) in EnumerateOwners())
        {
            if (!Enum.IsDefined(owner))
            {
                errors.Add($"{name} has unsupported owner '{owner}'.");
            }
        }
    }

    private void ValidateAccountAuthority(List<string> errors)
    {
        if (IdentityLifecycleOwner != NotificationOwnership.AccountAuthority)
        {
            errors.Add("IdentityLifecycleOwner must be AccountAuthority because credential-token lifecycle email is owned by the account authority.");
        }

        if (!Enum.IsDefined(DefaultAccountAuthorityKind) || DefaultAccountAuthorityKind == AccountAuthorityKind.None)
        {
            errors.Add("DefaultAccountAuthorityKind must name the account authority for identity lifecycle email.");
        }
    }

    private void ValidateDisabledCategories(List<string> errors)
    {
        ValidateDisabled(nameof(ProductLifecycleOwner), ProductLifecycleOwner, errors);
        ValidateDisabled(nameof(PlatformOperationsOwner), PlatformOperationsOwner, errors);
        ValidateDisabled(nameof(MarketingOwner), MarketingOwner, errors);
    }

    private static void ValidateDisabled(string name, NotificationOwnership owner, List<string> errors)
    {
        if (owner != NotificationOwnership.Disabled)
        {
            errors.Add($"{name} must be Disabled until a supported notification owner is implemented.");
        }
    }

    private void ValidateExternalProviderRules(List<string> errors)
    {
        if (ProviderInternalOwner == NotificationOwnership.ExternalWorkflowProvider
            && (!Enum.IsDefined(ProviderInternalProvider) || ProviderInternalProvider == ExternalWorkflowProviderKind.None))
        {
            errors.Add("ProviderInternalProvider must be set when provider-internal notifications are externally owned.");
        }

        if (ProviderInternalOwner == NotificationOwnership.AccountAuthority)
        {
            errors.Add("ProviderInternalOwner cannot be AccountAuthority.");
        }
    }

    private IEnumerable<(string Name, NotificationOwnership Owner)> EnumerateOwners()
    {
        yield return (nameof(IdentityLifecycleOwner), IdentityLifecycleOwner);
        yield return (nameof(ProductLifecycleOwner), ProductLifecycleOwner);
        yield return (nameof(ProviderInternalOwner), ProviderInternalOwner);
        yield return (nameof(PlatformOperationsOwner), PlatformOperationsOwner);
        yield return (nameof(MarketingOwner), MarketingOwner);
    }
}
