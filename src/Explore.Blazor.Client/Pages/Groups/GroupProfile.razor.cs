// ABOUTME: Group profile code-behind for identity, appearance, and accessibility announcements.
// ABOUTME: Restores persisted group state without product-specific content loading.

using Blazouter.Services;
using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Explore.Blazor.Client.Helpers;
using Explore.Blazor.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Explore.Blazor.Client.Pages.Groups;

public partial class GroupProfile
{
    [Inject] protected IGroupService GroupService { get; set; } = null!;
    [Inject] protected NavigationManager Navigation { get; set; } = null!;
    [Inject] protected RouterStateService RouterState { get; set; } = null!;
    [Inject] protected ILogger<GroupProfile> Logger { get; set; } = null!;
    [Inject] private IAccessibilityAnnouncerService AnnouncerService { get; set; } = default!;

    private Guid Id { get; set; }

    private HalResourceOfGroupDto? _group;
    private bool _isLoading = true;
    private AppearanceSettings _branding = new();
    private string _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(new AppearanceSettings(), "#334155");

    private string? _errorMessage;

    [PersistentState]
    public GroupProfileState? PersistedState { get; set; }

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
            _group = await GroupService.GetGroupDetailsAsync(Id);
            _branding = new AppearanceSettings
            {
                BackgroundColor = _group?.ActorBannerColor ?? string.Empty,
                ImageUri = _group?.ActorBannerPictureUri ?? string.Empty,
                BackgroundEffect = _group?.ActorBackgroundEffect ?? "None"
            };
            _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(_branding, "#334155");

            Logger.LogDebug("Loaded group {GroupId}", Id);
            PersistState();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load group data: {ex.Message}";
            Logger.LogError(ex, "Error loading group {GroupId}", Id);
        }
        finally
        {
            _isLoading = false;
        }

        if (_errorMessage != null)
            await AnnouncerService.AnnounceAssertiveAsync(_errorMessage);
        else if (_group == null)
            await AnnouncerService.AnnouncePoliteAsync("Group not found");

    }

    private string GetGroupPlaceholder()
    {
        return ImageHelper.GetOrganizationPlaceholder(null, _group?.FullName ?? "GRP");
    }

    private bool TryRestoreState()
    {
        if (PersistedState == null || PersistedState.GroupId != Id)
        {
            return false;
        }

        _group = PersistedState.Group;
        _branding = new AppearanceSettings
        {
            BackgroundColor = _group?.ActorBannerColor ?? string.Empty,
            ImageUri = _group?.ActorBannerPictureUri ?? string.Empty,
            BackgroundEffect = _group?.ActorBackgroundEffect ?? "None"
        };
        _bannerStyle = AppearanceStyleBuilder.BuildBannerStyle(_branding, "#334155");
        _isLoading = false;
        _errorMessage = null;

        return true;
    }

    private void PersistState()
    {
        PersistedState = new GroupProfileState
        {
            GroupId = Id,
            Group = _group
        };
    }

    public sealed class GroupProfileState
    {
        public Guid GroupId { get; init; }
        public HalResourceOfGroupDto? Group { get; init; }
    }
}
