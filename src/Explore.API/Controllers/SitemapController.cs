// ABOUTME: Root-level XML sitemap endpoint for public SEO discovery.
// ABOUTME: Publishes the tenant-aware static public route catalog.

using System.Xml.Linq;
using Asp.Versioning;
using Explore.API.Attributes;
using Explore.API.Hateoas;
using Explore.Application.Contracts.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Explore.API.Controllers;

[ApiVersion("0.1")]
[Route("sitemap.xml")]
[ApiController]
[EndpointClassification(EndpointClass.Public)]
public sealed class SitemapController(IPublicUrlBuilder publicUrlBuilder) : ControllerBase
{
    private static readonly IReadOnlyList<StaticSitemapEntry> StaticRoutes =
    [
        new("/", "daily", "1.0"),
        new("/welcome", "weekly", "0.8"),
        new("/about", "monthly", "0.5"),
        new("/contact", "monthly", "0.5"),
        new("/privacy", "yearly", "0.3"),
        new("/terms", "yearly", "0.3"),
        new("/community-guidelines", "yearly", "0.3")
    ];

    [HttpGet(Name = RouteNames.GetSitemap)]
    [AllowAnonymous]
    [OutputCache(PolicyName = "SitemapData")]
    [EndpointSummary("Get XML sitemap")]
    [EndpointDescription("Returns tenant-aware sitemap.xml with static public routes.")]
    [Produces("application/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult Get()
    {
        var baseUrl = publicUrlBuilder.GetBaseUrl().TrimEnd('/');

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset",
                StaticRoutes.Select(entry => CreateUrlElement(ns, BuildAbsoluteUrl(baseUrl, entry.Path), entry.ChangeFrequency, entry.Priority))));

        return Content(document.ToString(SaveOptions.DisableFormatting), "application/xml; charset=utf-8");
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
