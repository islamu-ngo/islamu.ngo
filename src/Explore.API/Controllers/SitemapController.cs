// ABOUTME: Root-level XML sitemap endpoint for public SEO discovery.
// ABOUTME: Publishes the tenant-aware static public route catalog.

using System.Xml.Linq;
using Asp.Versioning;
using Explore.API.Attributes;
using Explore.API.Hateoas;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Explore.API.Controllers;

[ApiVersion("0.1")]
[Route("sitemap.xml")]
[ApiController]
[EndpointClassification(EndpointClass.Public)]
public sealed class SitemapController(
    IMediator mediator,
    ITenantContext tenantContext,
    IPublicUrlBuilder publicUrlBuilder) : ControllerBase
{
    private const int BlogSitemapPageSize = 100;

    private static readonly IReadOnlyList<StaticSitemapEntry> StaticRoutes =
    [
        new("/", "daily", "1.0"),
        new("/welcome", "weekly", "0.8"),
        new("/blog", "weekly", "0.8"),
        new("/about", "monthly", "0.5"),
        new("/contact", "monthly", "0.5"),
        new("/community-guidelines", "yearly", "0.3")
    ];

    [HttpGet(Name = RouteNames.GetSitemap)]
    [AllowAnonymous]
    [OutputCache(PolicyName = "SitemapData")]
    [EndpointSummary("Get XML sitemap")]
    [EndpointDescription("Returns tenant-aware sitemap.xml with static public routes.")]
    [Produces("application/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ContentResult> Get(CancellationToken cancellationToken = default)
    {
        var baseUrl = publicUrlBuilder.GetBaseUrl().TrimEnd('/');
        var blogPaths = await GetPublishedBlogPaths(cancellationToken);

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset",
                StaticRoutes
                    .Select(entry => CreateUrlElement(ns, BuildAbsoluteUrl(baseUrl, entry.Path), entry.ChangeFrequency, entry.Priority))
                    .Concat(blogPaths.Select(path => CreateUrlElement(ns, BuildAbsoluteUrl(baseUrl, path), "weekly", "0.7")))));

        return Content(document.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
    }

    private async Task<IReadOnlyList<string>> GetPublishedBlogPaths(CancellationToken cancellationToken)
    {
        var firstPage = await GetPublishedBlogsPage(1, cancellationToken);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        AddCurrentPublishedBlogPaths(firstPage.Items, paths);

        for (var page = 2; page <= firstPage.TotalPages; page++)
        {
            var result = await GetPublishedBlogsPage(page, cancellationToken);
            AddCurrentPublishedBlogPaths(result.Items, paths);
        }

        return paths.ToList();
    }

    private Task<PaginatedResult<BlogSummaryDto>> GetPublishedBlogsPage(int page, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetPublishedBlogsQuery(tenantContext.TenantId, page, BlogSitemapPageSize), cancellationToken);
    }

    private static void AddCurrentPublishedBlogPaths(IEnumerable<BlogSummaryDto> blogs, HashSet<string> paths)
    {
        var now = DateTime.UtcNow;

        foreach (var blog in blogs)
        {
            if (string.IsNullOrWhiteSpace(blog.Slug) || blog.PublishedAtUtc is null || blog.PublishedAtUtc > now)
            {
                continue;
            }

            paths.Add(BuildBlogPath(blog.Slug));
        }
    }

    private static string BuildBlogPath(string slug)
    {
        var escapedSegments = slug
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        return "/blog/" + string.Join('/', escapedSegments);
    }

    private static XElement CreateUrlElement(
        XNamespace ns,
        string location,
        string changeFrequency,
        string priority)
    {
        var element = new XElement(ns + "url",
            new XElement(ns + "loc", location),
            new XElement(ns + "changefreq", changeFrequency),
            new XElement(ns + "priority", priority));

        return element;
    }

    private static string BuildAbsoluteUrl(string baseUrl, string path)
    {
        return string.Concat(baseUrl, path.StartsWith('/') ? path : string.Concat('/', path));
    }

    private sealed record StaticSitemapEntry(string Path, string ChangeFrequency, string Priority);
}
