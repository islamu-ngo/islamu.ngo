// ABOUTME: Organization profile code-behind for identity, appearance, and reviews.
// ABOUTME: Restores persisted profile state without product-specific content loading.

using Blazouter.Services;
using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Helpers;
using Explore.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Explore.Blazor.Client.Pages.Organizations;

public partial class OrganizationProfile
{
    [Inject] protected IOrganizationService OrganizationService { get; set; } = null!;
    [Inject] protected IOrganizationReviewService OrganizationReviewService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected RouterStateService RouterState { get; set; } = null!;
    [Inject] protected ILogger<OrganizationProfile> Logger { get; set; } = null!;

    private Guid Id { get; set; }

    private OrganizationDto? _organization;
    private bool _isLoading = true;
    private List<OrganizationReviewDto> _reviews = new();
    private AppearanceSettings _appearance = new();
    private string _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(new AppearanceSettings(), "#1f6feb");

    private string? _errorMessage;

    [PersistentState]
    public OrganizationProfileState? PersistedState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var idStr = RouterState.GetParam("id");
        if (Guid.TryParse(idStr, out var id))
        {
            Id = id;
        }

        if (TryRestoreState())
        {
            return;
        }

        _isLoading = true;
        _errorMessage = null;

        try
        {
            _organization = await OrganizationService.GetOrganizationByIdAsync(Id);

            if (_organization is not null)
            {
                Id = _organization.Id.GetValueOrDefault(Id);
                _reviews = (await OrganizationReviewService.GetReviewsByOrganizationId(Id)).ToList();
            }
            else
            {
                _reviews = new List<OrganizationReviewDto>();
            }

            _appearance = new AppearanceSettings
            {
                BackgroundColor = _organization?.ActorBackgroundColor ?? string.Empty,
                ImageUri = _organization?.ActorBannerPictureUri ?? string.Empty,
                BackgroundEffect = _organization?.ActorBackgroundEffect ?? "None"
            };
            _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(_appearance, "#1f6feb");

            Logger.LogDebug("Loaded organization {OrganizationId} with {ReviewCount} reviews", Id, _reviews.Count);

            PersistState();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load organization data: {ex.Message}";
            Logger.LogError(ex, "Error loading organization {OrganizationId}", Id);
        }
        finally
        {
            _isLoading = false;
        }

    }

    private void ShowAllReviews()
    {
        Navigation.NavigateTo($"/organization/reviews/{Id}");
    }

    private string GetOrganizationPlaceholder()
    {
        if (_organization == null)
            return ImageHelper.GetOrganizationPlaceholder(null, "ORG");

        return ImageHelper.GetOrganizationPlaceholder(null, _organization.FullName);
    }

    private string? GetProfileImageUri()
    {
        if (!string.IsNullOrWhiteSpace(_organization?.ActorProfilePictureUri))
        {
            return _organization.ActorProfilePictureUri;
        }

        return null;
    }

    private int GetAverageRating()
    {
        if (!_reviews.Any(r => r.Rating.HasValue)) return 0;
        return (int)Math.Round(_reviews.Where(r => r.Rating.HasValue).Average(r => r.Rating!.Value));
    }

    private bool TryRestoreState()
    {
        if (PersistedState == null || PersistedState.OrganizationId != Id)
        {
            return false;
        }

        _organization = PersistedState.Organization;
        _reviews = PersistedState.Reviews;
        _appearance = new AppearanceSettings
        {
            BackgroundColor = _organization?.ActorBackgroundColor ?? string.Empty,
            ImageUri = _organization?.ActorBannerPictureUri ?? string.Empty,
            BackgroundEffect = _organization?.ActorBackgroundEffect ?? "None"
        };
        _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(_appearance, "#1f6feb");
        _isLoading = false;
        _errorMessage = null;

        return true;
    }

    private void PersistState()
    {
        PersistedState = new OrganizationProfileState
        {
            OrganizationId = Id,
            Organization = _organization,
            Reviews = _reviews.ToList()
        };
    }

    public sealed class OrganizationProfileState
    {
        public Guid OrganizationId { get; init; }
        public OrganizationDto? Organization { get; init; }
        public List<OrganizationReviewDto> Reviews { get; init; } = new();
    }
}
