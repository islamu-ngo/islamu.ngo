// ABOUTME: REST API controller for public and editorial Blog HTTP endpoints.
// ABOUTME: Dispatches Blog CQRS requests and wraps read models with HAL resources.

using Asp.Versioning;
using Explore.API.Attributes;
using Explore.API.ExceptionHandling;
using Explore.API.Hateoas;
using Explore.API.Models;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Hateoas;
using Explore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Explore.API.Controllers;

/// <summary>
/// Blog API endpoints for published reads and authenticated editorial management.
/// </summary>
[ApiVersion("0.1")]
[Route("api/[controller]")]
[ApiController]
[Produces(HateoasConstants.JsonMediaType, HateoasConstants.HalJsonMediaType)]
public sealed class BlogController : ControllerBase
{
    private static readonly ApiValidationProblemDescriptor CreateValidationProblem = new(
        "blog",
        "Blog validation failed",
        "Blog creation failed.");

    private static readonly ApiValidationProblemDescriptor UpdateValidationProblem = new(
        "blog",
        "Blog validation failed",
        "Blog update failed.");

    private static readonly ApiValidationProblemDescriptor PublishValidationProblem = new(
        "blog",
        "Blog publication failed",
        "Blog publication failed.");

    private static readonly ApiValidationProblemDescriptor UnpublishValidationProblem = new(
        "blog",
        "Blog unpublication failed",
        "Blog unpublication failed.");

    private static readonly ApiValidationProblemDescriptor DeleteValidationProblem = new(
        "blog",
        "Blog deletion failed",
        "Blog deletion failed.");

    private static readonly ApiNotFoundProblemDescriptor BlogNotFoundProblem = new(
        "Blog not found",
        "Blog not found.");

    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly IResourceAssembler<BlogDetailDto, BlogSummaryDto> _resourceAssembler;

    public BlogController(
        IMediator mediator,
        ITenantContext tenantContext,
        IResourceAssembler<BlogDetailDto, BlogSummaryDto> resourceAssembler)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        _resourceAssembler = resourceAssembler;
    }

    /// <summary>
    /// Get published blog posts with pagination and optional filters.
    /// </summary>
    [AllowAnonymous]
    [EndpointClassification(EndpointClass.Public)]
    [HttpGet(Name = RouteNames.GetPublishedBlogs)]
    [EndpointSummary("Get published Blog posts")]
    [EndpointDescription("Retrieve a paginated list of published blog posts for the current tenant.")]
    [ProducesResponseType(typeof(HalCollectionResource<BlogSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [OutputCache(PolicyName = "ListData")]
    public async Task<ActionResult<HalCollectionResource<BlogSummaryDto>>> GetPublishedBlogs(
        [FromQuery] PaginationQueryRequest query,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? tagId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPublishedBlogsQuery(
            _tenantContext.TenantId,
            query.PageNumber,
            query.PageSize,
            categoryId,
            tagId,
            search), cancellationToken);

        var halResource = await _resourceAssembler.ToCollectionResource(
            result,
            RouteNames.GetPublishedBlogs,
            additionalRouteValues: new { categoryId, tagId, search },
            HttpContext);

        return Ok(halResource);
    }

    /// <summary>
    /// Get a published blog post by slug.
    /// </summary>
    [AllowAnonymous]
    [EndpointClassification(EndpointClass.Public)]
    [HttpGet("{slug}", Name = RouteNames.GetPublishedBlogBySlug)]
    [EndpointSummary("Get published Blog post")]
    [EndpointDescription("Retrieve a published blog post by slug for the current tenant.")]
    [ProducesResponseType(typeof(HalResource<BlogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [OutputCache(PolicyName = "DetailData")]
    public async Task<ActionResult<HalResource<BlogDetailDto>>> GetPublishedBlogBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var blog = await _mediator.Send(
            new GetPublishedBlogBySlugQuery(_tenantContext.TenantId, slug),
            cancellationToken);
        if (blog is null)
        {
            return this.ToNotFoundProblem(BlogNotFoundProblem);
        }

        var halResource = await _resourceAssembler.ToResource(blog, HttpContext);
        return Ok(halResource);
    }

    /// <summary>
    /// Get blog posts for authenticated editorial management.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpGet("manage", Name = RouteNames.GetBlogsForManagement)]
    [EndpointSummary("Get manageable Blog posts")]
    [EndpointDescription("Retrieve a paginated management list of blog posts for the current tenant.")]
    [ProducesResponseType(typeof(HalCollectionResource<BlogSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<HalCollectionResource<BlogSummaryDto>>> GetBlogsForManagement(
        [FromQuery] PaginationQueryRequest query,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBlogsForManagementQuery(
            _tenantContext.TenantId,
            query.PageNumber,
            query.PageSize,
            search), cancellationToken);

        var halResource = await _resourceAssembler.ToCollectionResource(
            result,
            RouteNames.GetBlogsForManagement,
            additionalRouteValues: new { search },
            HttpContext);

        return Ok(halResource);
    }

    /// <summary>
    /// Get a blog post by ID for authenticated editorial management.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpGet("manage/{id:guid}", Name = RouteNames.GetBlogByIdForManagement)]
    [EndpointSummary("Get manageable Blog post")]
    [EndpointDescription("Retrieve a blog post by ID for authenticated editorial management.")]
    [ProducesResponseType(typeof(HalResource<BlogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HalResource<BlogDetailDto>>> GetBlogByIdForManagement(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var blog = await _mediator.Send(
            new GetBlogByIdForManagementQuery(_tenantContext.TenantId, id),
            cancellationToken);
        if (blog is null)
        {
            return this.ToNotFoundProblem(BlogNotFoundProblem);
        }

        var halResource = await _resourceAssembler.ToResource(blog, HttpContext);
        return Ok(halResource);
    }

    /// <summary>
    /// Create a blog post.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpPost(Name = RouteNames.CreateBlog)]
    [EndpointSummary("Create Blog post")]
    [EndpointDescription("Create a draft blog post for the current tenant.")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BaseCommandResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BaseCommandResponse<Guid>>> Create(
        [FromBody] CreateBlogDto blog,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new CreateBlogCommand(_tenantContext.TenantId, blog),
            cancellationToken);
        if (!response.Success)
        {
            return this.ToCommandValidationProblem(response, CreateValidationProblem);
        }

        return CreatedAtRoute(RouteNames.GetBlogByIdForManagement, new { id = response.Id }, response);
    }

    /// <summary>
    /// Update a blog post.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpPatch("{id:guid}", Name = RouteNames.UpdateBlog)]
    [EndpointSummary("Update Blog post")]
    [EndpointDescription("Update an existing blog post for the current tenant.")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BaseCommandResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseCommandResponse<Guid>>> Update(
        Guid id,
        [FromBody] UpdateBlogDto blog,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new UpdateBlogCommand(_tenantContext.TenantId, id, blog),
            cancellationToken);
        if (!response.Success)
        {
            return this.ToCommandValidationProblem(response, UpdateValidationProblem);
        }

        return Ok(response);
    }

    /// <summary>
    /// Publish a blog post.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpPost("{id:guid}/publish", Name = RouteNames.PublishBlog)]
    [EndpointSummary("Publish Blog post")]
    [EndpointDescription("Publish a draft blog post for the current tenant.")]
    [ProducesResponseType(typeof(BaseCommandResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseCommandResponse<Guid>>> Publish(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new PublishBlogCommand(_tenantContext.TenantId, id),
            cancellationToken);
        if (!response.Success)
        {
            return this.ToCommandValidationProblem(response, PublishValidationProblem);
        }

        return Ok(response);
    }

    /// <summary>
    /// Unpublish a blog post.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpPost("{id:guid}/unpublish", Name = RouteNames.UnpublishBlog)]
    [EndpointSummary("Unpublish Blog post")]
    [EndpointDescription("Move a published blog post back to draft for the current tenant.")]
    [ProducesResponseType(typeof(BaseCommandResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BaseCommandResponse<Guid>>> Unpublish(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new UnpublishBlogCommand(_tenantContext.TenantId, id),
            cancellationToken);
        if (!response.Success)
        {
            return this.ToCommandValidationProblem(response, UnpublishValidationProblem);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a blog post.
    /// </summary>
    [Authorize]
    [EndpointClassification(EndpointClass.Authenticated)]
    [HttpDelete("{id:guid}", Name = RouteNames.DeleteBlog)]
    [EndpointSummary("Delete Blog post")]
    [EndpointDescription("Delete a blog post for the current tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new DeleteBlogCommand(_tenantContext.TenantId, id),
            cancellationToken);
        if (!response.Success)
        {
            return this.ToCommandValidationProblem(response, DeleteValidationProblem);
        }

        return NoContent();
    }
}
