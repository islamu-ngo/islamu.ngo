// ABOUTME: Component tests for Phase 9.7 + 9.9 custom-property governance admin UI sections.
// ABOUTME: Verifies exposure-grid and governance-report loading, error, and success states.

using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Contracts.Services.CustomProperties;
using Explore.Blazor.Client.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Explore.Blazor.Client.Tests.Pages.Admin;

public class CustomPropertyGovernanceTests : IDisposable
{
    private readonly BlazorTestContext _ctx;
    private readonly ICustomPropertyAdminService _adminService;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CustomPropertyGovernanceTests()
    {
        _ctx = new BlazorTestContext();
        _adminService = Substitute.For<ICustomPropertyAdminService>();

        _ctx.Services.AddSingleton(_adminService);
        _ctx.Services.AddSingleton(Substitute.For<IDialogService>());
        _ctx.Services.AddSingleton(Substitute.For<ISnackbar>());

        _ctx.SetAuthenticatedUser(Guid.NewGuid(), "Admin User", "admin@example.com", _tenantId);
    }

    public void Dispose() => _ctx.Dispose();

    private IRenderedComponent<DynamicComponent> Render(string typeName)
    {
        var type = typeof(Explore.Blazor.Client.Services.CustomPropertyAdminService).Assembly
            .GetType($"Explore.Blazor.Client.Pages.Admin.CustomProperties.Components.{typeName}")
            ?? throw new InvalidOperationException($"Component {typeName} not found");

        return _ctx.RenderMudComponent<DynamicComponent>(p => p.Add(x => x.Type, type));
    }

    private static CustomPropertyDefinitionListDto SampleDefinition(string key = "venue_capacity") => new()
    {
        Id = Guid.NewGuid(),
        EntityTypeName = (int)EntityTypeName.Organization,
        Namespace = "tenant",
        Key = key,
        DisplayName = "Venue Capacity",
        PropertyType = (int)PropertyType.Number,
        IsRequired = false,
        IsMulti = false,
        IsActive = true,
        SortOrder = 0,
        ExposureLevel = (int)ExposureLevel.Public,
        IsSearchable = true,
        IsFilterable = true,
        IsExportable = false,
        IsModerationRelevant = false,
        IsAnalyticsRelevant = true,
        IsSystemOwned = false,
        OptionCount = 0
    };

    // ── ExposureGovernanceSection (Task 9.7 / 9.9) ──

    [Test]
    public async Task ExposureSection_ShowsLoadingState_WhilePending()
    {
        var pending = new TaskCompletionSource<PaginatedResult<CustomPropertyDefinitionListDto>>();
        _adminService.GetDefinitionsAsync(Arg.Any<EntityTypeName>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(pending.Task);

        var cut = Render("ExposureGovernanceSection");

        await Assert.That(cut.Markup).Contains("Loading definitions");

        pending.TrySetResult(PaginatedResult<CustomPropertyDefinitionListDto>.Empty());
    }

    [Test]
    public async Task ExposureSection_RendersDefinitions_AfterLoad()
    {
        var items = new List<CustomPropertyDefinitionListDto> { SampleDefinition() };
        _adminService.GetDefinitionsAsync(Arg.Any<EntityTypeName>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PaginatedResult<CustomPropertyDefinitionListDto>
            {
                Items = items,
                PageNumber = 1,
                PageSize = 200,
                TotalCount = 1
            });

        var cut = Render("ExposureGovernanceSection");
        cut.WaitForState(() => cut.Markup.Contains("Venue Capacity"), TimeSpan.FromSeconds(3));

        await Assert.That(cut.Markup).Contains("Venue Capacity");
        await Assert.That(cut.Markup).Contains("venue_capacity");
    }

    [Test]
    public async Task ExposureSection_ShowsErrorAlert_WhenLoadFails()
    {
        _adminService.GetDefinitionsAsync(Arg.Any<EntityTypeName>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("network down"));

        var cut = Render("ExposureGovernanceSection");
        cut.WaitForState(() => cut.Markup.Contains("Failed to load definitions", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(3));

        await Assert.That(cut.Markup).Contains("Failed to load definitions: network down");
    }

    // ── GovernanceReportSection ──

    [Test]
    public async Task GovernanceReport_ShowsLoadingState_WhilePending()
    {
        var pending = new TaskCompletionSource<PaginatedResult<CustomPropertyGovernanceRowDto>>();
        _adminService.GetGovernanceReportAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<PromotionRecommendation?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(pending.Task);

        var cut = Render("GovernanceReportSection");

        await Assert.That(cut.Markup).Contains("Generating governance report");

        pending.TrySetResult(PaginatedResult<CustomPropertyGovernanceRowDto>.Empty());
    }

    [Test]
    public async Task GovernanceReport_ShowsErrorAlert_WhenRequestFails()
    {
        _adminService.GetGovernanceReportAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<PromotionRecommendation?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        var cut = Render("GovernanceReportSection");
        cut.WaitForState(() => cut.Markup.Contains("Failed to load", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(3));

        await Assert.That(cut.Markup).Contains("boom");
    }

    [Test]
    public async Task GovernanceReport_RendersRow_AfterLoad()
    {
        var row = new CustomPropertyGovernanceRowDto
        {
            TenantId = Guid.NewGuid(),
            Namespace = "tenant",
            Key = "venue_capacity",
            DisplayName = "Venue Capacity",
            EntityScope = "Organization",
            PropertyType = "Number",
            ExposureLevel = ExposureLevel.Public,
            IsSearchable = true,
            IsFilterable = true,
            IsExportable = false,
            IsModerationRelevant = false,
            IsAnalyticsRelevant = false,
            IsSystemOwned = false,
            ActiveInstanceCount = 42,
            LastUsedAt = DateTimeOffset.UtcNow,
            Recommendation = PromotionRecommendation.ConsiderLayer2Promotion
        };

        _adminService.GetGovernanceReportAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<PromotionRecommendation?>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PaginatedResult<CustomPropertyGovernanceRowDto>
            {
                Items = new List<CustomPropertyGovernanceRowDto> { row },
                PageNumber = 1,
                PageSize = 50,
                TotalCount = 1
            });

        var cut = Render("GovernanceReportSection");
        cut.WaitForState(() => cut.Markup.Contains("Venue Capacity"), TimeSpan.FromSeconds(3));

        await Assert.That(cut.Markup).Contains("venue_capacity");
        await Assert.That(cut.Markup).Contains("42");
    }

}
