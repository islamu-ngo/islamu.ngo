// ABOUTME: Code-behind for the static public SiteMegaMenu disclosure and mobile drawer state.
// ABOUTME: Handles viewport switching, focus restoration, route dismissal, and delayed desktop close.

#nullable enable

using Explore.Blazor.Client.Contracts.Services.Accessibility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Services;

namespace Explore.Blazor.Client.Layout;

public partial class SiteMegaMenu : ComponentBase, IAsyncDisposable, IBrowserViewportObserver
{
    private const int MobileBreakpointPx = 768;
    private const int DesktopCloseDelayMs = 160;
    private const int DesktopExitDelayMs = 180;
    private static readonly IReadOnlyList<MenuSection> MenuSections =
    [
        new("discover", "Discover", [new("Home", "/", "home"), new("Blog", "/blog", "blog")]),
        new("about", "About ISLAMU", [new("About", "/about", "about"), new("Contact", "/contact", "contact")]),
        new("community", "Community", [new("Community guidelines", "/community-guidelines", "community-guidelines")])
    ];

    private bool _isMobile;
    private bool _mobileOpen;
    private bool _viewportSubscribed;
    private bool _disposed;
    private bool _renderDesktopPanel;
    private bool _isDesktopClosing;
    private string? _openSectionId;
    private CancellationTokenSource? _desktopCloseCts;
    private CancellationTokenSource? _desktopExitCts;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    [Inject]
    private IAccessibilityFocusService AccessibilityFocusService { get; set; } = null!;

    [Parameter]
    public EventCallback OnOpening { get; set; }

    protected IReadOnlyList<MenuSection> Sections => MenuSections;

    protected bool IsDesktop => !_isMobile;

    protected bool HasOpenSection => _openSectionId is not null;

    protected bool ShouldRenderDesktopPanel => _renderDesktopPanel;

    protected string? OpenSectionId => _openSectionId;

    protected string PanelClass => _isDesktopClosing
        ? "site-mega-menu__panel site-mega-menu__panel--closing"
        : "site-mega-menu__panel site-mega-menu__panel--open";

    Guid IBrowserViewportObserver.Id { get; } = Guid.NewGuid();

    ResizeOptions IBrowserViewportObserver.ResizeOptions { get; } = new()
    {
        ReportRate = 250,
        NotifyOnBreakpointOnly = false
    };

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_viewportSubscribed)
        {
            _viewportSubscribed = true;
            await BrowserViewportService.SubscribeAsync(this, fireImmediately: true);
        }
    }

    protected async Task ToggleDesktopSectionAsync(string sectionId)
    {
        if (_openSectionId == sectionId)
        {
            await CloseDesktopAsync(restoreFocus: true);
            return;
        }

        await OpenDesktopSectionAsync(sectionId);
    }

    protected async Task OpenDesktopSectionAsync(string sectionId)
    {
        if (_isMobile || _disposed)
        {
            return;
        }

        CancelDesktopClose();
        CancelDesktopExit();
        var wasClosed = _openSectionId is null;
        if (wasClosed)
        {
            await AccessibilityFocusService.SaveFocusAsync();
            if (_disposed)
            {
                return;
            }
        }

        _openSectionId = sectionId;
        _renderDesktopPanel = true;
        _isDesktopClosing = false;

        if (wasClosed)
        {
            await OnOpening.InvokeAsync();
        }
    }

    protected async Task CloseDesktopAsync(bool restoreFocus)
    {
        if (_disposed)
        {
            return;
        }

        var sectionId = _openSectionId;
        CancelDesktopClose();
        if (sectionId is null && !_renderDesktopPanel)
        {
            return;
        }

        _openSectionId = null;
        _isDesktopClosing = _renderDesktopPanel;

        if (restoreFocus && sectionId is not null)
        {
            await AccessibilityFocusService.FocusAsync($"#{TriggerId(sectionId)}");
        }

        if (_renderDesktopPanel)
        {
            ScheduleDesktopPanelRemoval();
        }
    }

    protected async Task ToggleMobileMenuAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_mobileOpen)
        {
            await CloseMobileAsync(restoreFocus: true);
            return;
        }

        await AccessibilityFocusService.SaveFocusAsync();
        if (_disposed)
        {
            return;
        }

        _mobileOpen = true;
        await OnOpening.InvokeAsync();
    }

    protected async Task HandleMobileOpenChangedAsync(bool open)
    {
        if (open == _mobileOpen)
        {
            return;
        }

        if (open)
        {
            await ToggleMobileMenuAsync();
            return;
        }

        await CloseMobileAsync(restoreFocus: true);
    }

    protected async Task CloseMobileAsync(bool restoreFocus)
    {
        if (_disposed)
        {
            return;
        }

        if (!_mobileOpen)
        {
            return;
        }

        _mobileOpen = false;

        if (restoreFocus)
        {
            await AccessibilityFocusService.RestoreFocusAsync("#site-mega-menu-mobile-toggle");
        }
    }

    protected async Task CloseForNavigationAsync()
    {
        await CloseDesktopAsync(restoreFocus: false);
        await CloseMobileAsync(restoreFocus: false);
    }

    public async Task DismissAsync()
    {
        await CloseForNavigationAsync();
        if (!_disposed)
        {
            StateHasChanged();
        }
    }

    protected async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (!string.Equals(args.Key, "Escape", StringComparison.Ordinal))
        {
            return;
        }

        if (_mobileOpen)
        {
            await CloseMobileAsync(restoreFocus: true);
            return;
        }

        await CloseDesktopAsync(restoreFocus: true);
    }

    protected Task ScheduleDesktopCloseAsync()
    {
        if (_openSectionId is null || _disposed)
        {
            return Task.CompletedTask;
        }

        CancelDesktopClose();
        var cts = new CancellationTokenSource();
        _desktopCloseCts = cts;

        _ = InvokeAsync(async () =>
        {
            try
            {
                await Task.Delay(DesktopCloseDelayMs, cts.Token);
                if (_disposed || cts.IsCancellationRequested || !ReferenceEquals(_desktopCloseCts, cts))
                {
                    return;
                }

                _desktopCloseCts = null;
                await CloseDesktopAsync(restoreFocus: false);
                if (!_disposed && !cts.IsCancellationRequested)
                {
                    await InvokeAsync(() =>
                    {
                        if (!_disposed)
                        {
                            StateHasChanged();
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (ReferenceEquals(_desktopCloseCts, cts))
                {
                    _desktopCloseCts = null;
                }

                cts.Dispose();
            }
        });

        return Task.CompletedTask;
    }

    protected void CancelDesktopClose()
    {
        CancelDesktopDelay(ref _desktopCloseCts);
    }

    private void CancelDesktopExit()
    {
        CancelDesktopDelay(ref _desktopExitCts);
    }

    private static void CancelDesktopDelay(ref CancellationTokenSource? field)
    {
        var cts = field;
        if (cts is null)
        {
            return;
        }

        field = null;
        try
        {
            cts.Cancel();
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
            return;
        }
    }

    private void ScheduleDesktopPanelRemoval()
    {
        CancelDesktopExit();
        if (_disposed)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        _desktopExitCts = cts;

        _ = InvokeAsync(async () =>
        {
            try
            {
                await Task.Delay(DesktopExitDelayMs, cts.Token);
                if (_disposed || cts.IsCancellationRequested || !ReferenceEquals(_desktopExitCts, cts))
                {
                    return;
                }

                _desktopExitCts = null;
                _renderDesktopPanel = false;
                _isDesktopClosing = false;
                if (!_disposed)
                {
                    await InvokeAsync(() =>
                    {
                        if (!_disposed)
                        {
                            StateHasChanged();
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                if (ReferenceEquals(_desktopExitCts, cts))
                {
                    _desktopExitCts = null;
                }

                cts.Dispose();
            }
        });
    }

    protected static string TriggerId(string sectionId) => $"site-mega-menu-trigger-{sectionId}";

    protected static string PanelId(string sectionId) => $"site-mega-menu-panel-{sectionId}";

    protected static string HeadingId(string sectionId) => $"site-mega-menu-heading-{sectionId}";

    protected string TriggerClass(string sectionId)
        => _openSectionId == sectionId && IsDesktop
            ? "site-mega-menu__trigger site-mega-menu__trigger--open"
            : "site-mega-menu__trigger";

    protected string PanelSectionClass(string sectionId)
        => _openSectionId == sectionId
            ? "site-mega-menu__section site-mega-menu__section--selected"
            : "site-mega-menu__section";

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _ = InvokeAsync(async () =>
        {
            await CloseForNavigationAsync();
            if (!_disposed)
            {
                StateHasChanged();
            }
        });
    }

    Task IBrowserViewportObserver.NotifyBrowserViewportChangeAsync(BrowserViewportEventArgs args)
    {
        var isMobile = args.BrowserWindowSize.Width < MobileBreakpointPx;
        if (_isMobile == isMobile)
        {
            return Task.CompletedTask;
        }

        return InvokeAsync(async () =>
        {
            if (_disposed)
            {
                return;
            }

            _isMobile = isMobile;
            await CloseForNavigationAsync();
            if (!_disposed)
            {
                StateHasChanged();
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        Navigation.LocationChanged -= OnLocationChanged;
        CancelDesktopClose();
        CancelDesktopExit();

        if (_viewportSubscribed)
        {
            await BrowserViewportService.UnsubscribeAsync(this);
        }
    }

    protected sealed record MenuSection(string Id, string Title, IReadOnlyList<MenuItem> Items);

    protected sealed record MenuItem(string Label, string Href, string TestId);
}
