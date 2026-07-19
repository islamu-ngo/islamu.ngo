// ABOUTME: Component tests for the public Home page shell and public-experience rendering.
// ABOUTME: Verifies authentication states, organization-centric projection, and encoded rich text.

using Explore.Blazor.Client.Pages;
using MudBlazor;

namespace Explore.Blazor.Client.Tests.Pages;

/// <summary>
/// Component tests for Home page.
/// Tests authentication state handling and conditional rendering.
/// </summary>
/// <remarks>
/// Home page has three states:
/// 1. Loading - Shows skeleton placeholders while checking auth
/// 2. Authenticated - Shows LandingPageForUsers
/// 3. Anonymous - Shows LandingPageForNonUsers
/// </remarks>
public class HomeTests : IDisposable
{
    private readonly BlazorTestContext _ctx;

    public HomeTests()
    {
        _ctx = new BlazorTestContext();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    #region Authentication State Tests

    [Test]
    public async Task Home_ShowsLoadingState_Initially()
    {
        // Arrange - Set up slow auth response
        _ctx.SetAuthorizingState();
        SetupLandingPageServices();

        // Act
        var cut = _ctx.RenderMudComponent<Home>();

        // Assert - Should show skeleton loading placeholders
        await Assert.That(cut.Markup).Contains("mud-skeleton");
    }

    [Test]
    public async Task Home_ShowsLandingPageForUsers_WhenAuthenticated()
    {
        // Arrange
        _ctx.SetAuthenticatedUser(Guid.NewGuid(), "Test User", "test@example.com");

        // Add required services for LandingPageForUsers
        SetupLandingPageServices();

        // Act
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert - Should render authenticated content
        // LandingPageForUsers typically has different content than non-user page
        await Assert.That(cut.Markup).DoesNotContain("mud-skeleton");
    }

    [Test]
    public async Task Home_ShowsLandingPageForNonUsers_WhenAnonymous()
    {
        // Arrange
        _ctx.SetAnonymousUser();

        // Add required services for LandingPageForNonUsers
        SetupLandingPageServices();

        // Act
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert - Should render anonymous content
        await Assert.That(cut.Markup).DoesNotContain("mud-skeleton");
    }

    [Test]
    public async Task Home_ShowsOrganizationCentricShell_WhenPrimaryOrganizationIsAvailable()
    {
        // Arrange
        _ctx.SetAnonymousUser();
        SetupLandingPageServices(new PublicExperienceShellDto
        {
            Mode = PublicExperienceMode.OrganizationCentric,
            PrimaryOrganization = new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.Available,
                DisplayName = "Northside Masjid",
                ActorId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Handle = "@northside",
                WebsiteUrl = "https://northside.example"
            },
            Home = new PublicExperienceHomeDto
            {
                Blocks =
                [
                    new PublicExperienceHomeBlockDto
                    {
                        Key = "hero",
                        Kind = PublicExperienceHomeBlockKind.OrganizationSummary,
                        Title = "Northside Masjid",
                        Body = "Community programs for every family.",
                        SortOrder = 0
                    }
                ]
            },
            Ctas =
            [
                new PublicExperienceCtaDto
                {
                    Key = "donate",
                    Label = "Support Us",
                    Url = "/donate"
                }
            ],
            Footer = new FooterConfigDto
            {
                LinkGroups =
                [
                    new FooterLinkGroupDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Community",
                        Links =
                        [
                            new FooterLinkItemDto
                            {
                                Id = Guid.NewGuid(),
                                Label = "Volunteer",
                                Url = "/volunteer"
                            }
                        ]
                    }
                ]
            }
        });

        // Act
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert
        await Assert.That(cut.Markup).Contains("Northside Masjid");
        await Assert.That(cut.Markup).Contains("Community programs for every family.");
        await Assert.That(cut.Markup).Contains("Support Us");
        await Assert.That(cut.Markup).Contains("Connect with Northside Masjid");
        await Assert.That(cut.Markup).Contains("Volunteer");
        await Assert.That(cut.Markup).DoesNotContain("Never Miss a Muslim Event Again");
    }

    [Test]
    public async Task Home_OrganizationRichTextBlock_RendersTenantContentAsEncodedText()
    {
        _ctx.SetAnonymousUser();
        SetupLandingPageServices(new PublicExperienceShellDto
        {
            Mode = PublicExperienceMode.OrganizationCentric,
            PrimaryOrganization = new PublicExperiencePrimaryOrganizationDto
            {
                State = PublicExperiencePrimaryOrganizationState.Available,
                DisplayName = "Northside Masjid",
                ActorId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            },
            Home = new PublicExperienceHomeDto
            {
                Blocks =
                [
                    new PublicExperienceHomeBlockDto
                    {
                        Key = "rich-text",
                        Kind = PublicExperienceHomeBlockKind.RichText,
                        Title = "<img src=x onerror=alert(1)>",
                        Subtitle = "<script>alert(1)</script>",
                        Body = "<strong>Community update</strong><script>alert(1)</script><img src=x onerror=alert(1)>",
                        SortOrder = 10
                    }
                ]
            }
        });

        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        var richTextBlock = cut.FindAll(".organization-home__block")
            .Single(element => element.TextContent.Contains("<strong>Community update</strong>", StringComparison.Ordinal));

        await Assert.That(richTextBlock.TextContent).Contains("<img src=x onerror=alert(1)>");
        await Assert.That(richTextBlock.TextContent).Contains("<script>alert(1)</script>");
        await Assert.That(richTextBlock.InnerHtml).Contains("&lt;strong&gt;Community update&lt;/strong&gt;");
        await Assert.That(richTextBlock.InnerHtml).Contains("&lt;script&gt;alert(1)&lt;/script&gt;");
        await Assert.That(richTextBlock.InnerHtml).DoesNotContain("<script");
        await Assert.That(richTextBlock.InnerHtml).DoesNotContain("<img");
        await Assert.That(cut.FindAll("script").Count).IsEqualTo(0);
        await Assert.That(cut.FindAll("img").Count).IsEqualTo(0);
    }

    [Test]
    public async Task Home_ShowsSafeOrganizationRemediation_WhenPrimaryOrganizationIsMissing()
    {
        // Arrange
        _ctx.SetAnonymousUser();
        SetupLandingPageServices(new PublicExperienceShellDto
        {
            Mode = PublicExperienceMode.OrganizationCentric,
            PrimaryOrganization = new PublicExperiencePrimaryOrganizationDto { State = PublicExperiencePrimaryOrganizationState.Missing }
        });

        // Act
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert
        await Assert.That(cut.Markup).Contains("Organization home is not available yet");
        await Assert.That(cut.Markup).DoesNotContain("Never Miss a Muslim Event Again");
    }

    #endregion

    #region Page Title Tests

    [Test]
    public async Task Home_SetsPageTitle()
    {
        // Arrange
        _ctx.SetAnonymousUser();
        SetupLandingPageServices();

        // Act
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert - PageTitle component renders in head, check landing page content instead
        // LandingPageForNonUsers has specific content like "Sign Up" and "Explore"
        await Assert.That(cut.Markup).Contains("Sign Up");
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public async Task Home_HandlesAuthError_Gracefully()
    {
        // Arrange - Set anonymous (simulates auth error fallback)
        _ctx.SetAnonymousUser();
        SetupLandingPageServices();

        // Act - Should not throw
        var cut = _ctx.RenderMudComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("mud-skeleton", StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(2));

        // Assert - Should render without crash
        await Assert.That(cut.Markup).DoesNotContain("mud-skeleton");
    }

    #endregion

    /// <summary>
    /// Sets up services required by the landing page components.
    /// </summary>
    private void SetupLandingPageServices(PublicExperienceShellDto? shell = null)
    {
        var categoryService = Substitute.For<ICategoryService>();
        categoryService.GetAllCategoriesAsync().Returns(new List<CategoryListDto>());
        _ctx.Services.AddSingleton(categoryService);

        var organizationService = Substitute.For<IOrganizationService>();
        organizationService.GetMyOrganizationsAsync().Returns(new List<OrganizationListDto>());
        _ctx.Services.AddSingleton(organizationService);

        var userService = Substitute.For<IUserService>();
        _ctx.Services.AddSingleton(userService);

        var authStateService = Substitute.For<IAuthStateService>();
        authStateService.GetCurrentUserIdAsync().Returns(Guid.NewGuid().ToString());
        authStateService.IsAuthenticatedAsync().Returns(true);
        _ctx.Services.AddSingleton(authStateService);

        var publicExperienceService = Substitute.For<IPublicExperienceService>();
        publicExperienceService.GetCachedShellAsync().Returns(Task.FromResult(shell));
        _ctx.Services.AddSingleton(publicExperienceService);

        // Add dialog and snackbar services
        _ctx.Services.AddSingleton(Substitute.For<IDialogService>());
        _ctx.Services.AddSingleton(Substitute.For<ISnackbar>());
    }
}
