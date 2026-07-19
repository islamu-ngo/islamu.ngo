// ABOUTME: Direct controller tests for sitemap XML generation and published Blog URL discovery.
// ABOUTME: Verifies tenant-scoped MediatR paging, URL escaping, de-duplication, and URL-builder contract cleanup.

using System.Reflection;
using System.Xml.Linq;
using Explore.API.Controllers;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Event.Api.IntegrationTests.Features;

public sealed class SitemapControllerTests
{
    private const string BaseUrl = "https://www.islamu.test";

    [Test]
    public async Task Get_ShouldRenderStaticRoutesAndPublishedBlogUrls()
    {
        var tenantId = Guid.CreateVersion7();
        var cancellationToken = new CancellationTokenSource().Token;
        var mediator = Substitute.For<IMediator>();
        var firstPage = Page([
            Blog("zakat basics", DateTime.UtcNow.AddDays(-2)),
            Blog("zakat basics", DateTime.UtcNow.AddDays(-1)),
            Blog("fiqh/intentions and dues", DateTime.UtcNow.AddDays(-1)),
            Blog(" ", DateTime.UtcNow.AddDays(-1))
        ], totalCount: 101, pageNumber: 1);
        var secondPage = Page([
            Blog("draft-post", null),
            Blog("future-post", DateTime.UtcNow.AddDays(1)),
            Blog("current-post", DateTime.UtcNow.AddDays(-1))
        ], totalCount: 101, pageNumber: 2);
        mediator.Send(Arg.Any<GetPublishedBlogsQuery>(), Arg.Any<CancellationToken>()).Returns(firstPage, secondPage);

        var result = await CreateController(tenantId, mediator).Get(cancellationToken);
        var locations = ReadLocations(result);

        await Assert.That(result.ContentType).IsEqualTo("application/xml; charset=utf-8");
        await Assert.That(locations).Contains(BaseUrl + "/");
        await Assert.That(locations).Contains(BaseUrl + "/welcome");
        await Assert.That(locations).Contains(BaseUrl + "/blog");
        await Assert.That(locations).Contains(BaseUrl + "/about");
        await Assert.That(locations).Contains(BaseUrl + "/contact");
        await Assert.That(locations).Contains(BaseUrl + "/community-guidelines");
        await Assert.That(locations).DoesNotContain(BaseUrl + "/privacy");
        await Assert.That(locations).DoesNotContain(BaseUrl + "/terms");
        await Assert.That(locations).Contains(BaseUrl + "/blog/zakat%20basics");
        await Assert.That(locations).Contains(BaseUrl + "/blog/fiqh/intentions%20and%20dues");
        await Assert.That(locations).Contains(BaseUrl + "/blog/current-post");
        await Assert.That(locations).DoesNotContain(BaseUrl + "/blog/draft-post");
        await Assert.That(locations).DoesNotContain(BaseUrl + "/blog/future-post");
        await Assert.That(locations.Count(location => location == BaseUrl + "/blog/zakat%20basics")).IsEqualTo(1);
        await Assert.That(ReadPriorities(result).Count(priority => priority == "0.7")).IsEqualTo(3);

        await mediator.Received(1).Send(
            Arg.Is<GetPublishedBlogsQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.Page == 1 &&
                query.PageSize == 100),
            cancellationToken);
        await mediator.Received(1).Send(
            Arg.Is<GetPublishedBlogsQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.Page == 2 &&
                query.PageSize == 100),
            cancellationToken);
        await mediator.DidNotReceive().Send(
            Arg.Is<GetPublishedBlogsQuery>(query => query != null && query.Page == 3),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task PublicUrlBuilder_ShouldNotExposeEventUrlMethod()
    {
        await Assert.That(typeof(IPublicUrlBuilder).GetMethod("GetEventUrl", BindingFlags.Public | BindingFlags.Instance)).IsNull();
    }

    private static SitemapController CreateController(Guid tenantId, IMediator mediator)
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(tenantId);

        var publicUrlBuilder = Substitute.For<IPublicUrlBuilder>();
        publicUrlBuilder.GetBaseUrl().Returns(BaseUrl);

        return new SitemapController(mediator, tenantContext, publicUrlBuilder);
    }

    private static PaginatedResult<BlogSummaryDto> Page(List<BlogSummaryDto> items, int totalCount, int pageNumber)
    {
        return PaginatedResult<BlogSummaryDto>.Create(items, totalCount, pageNumber, 100);
    }

    private static BlogSummaryDto Blog(string slug, DateTime? publishedAtUtc)
    {
        return new BlogSummaryDto
        {
            Id = Guid.CreateVersion7(),
            TenantId = Guid.CreateVersion7(),
            Title = slug,
            Slug = slug,
            PublishedAtUtc = publishedAtUtc
        };
    }

    private static IReadOnlyList<string> ReadLocations(ContentResult result)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        return XDocument.Parse(result.Content!)
            .Descendants(ns + "loc")
            .Select(element => element.Value)
            .ToList();
    }

    private static IReadOnlyList<string> ReadPriorities(ContentResult result)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        return XDocument.Parse(result.Content!)
            .Descendants(ns + "priority")
            .Select(element => element.Value)
            .ToList();
    }
}
