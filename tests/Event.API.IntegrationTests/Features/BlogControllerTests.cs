// ABOUTME: Integration tests for Blog API routing and authorization posture.
// ABOUTME: Pins public read routes plus authenticated management and mutation boundaries.

using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Event.Api.IntegrationTests.Fixtures;
using Explore.API.Controllers;
using Explore.API.Hateoas;
using Explore.API.Models;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Hateoas;
using Explore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NSubstitute;

namespace Event.Api.IntegrationTests.Features;

[NotInParallel("ApiTestFixture")]
[ClassDataSource<ApiTestFixture>(Shared = SharedType.PerAssembly)]
public sealed class BlogControllerTests
{
    private const string BaseUrl = "/api/blog";
    private readonly ApiTestFixture _fixture;

    public BlogControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Test]
    public async Task ManagementReadActions_ShouldNotUseOutputCache()
    {
        var controllerType = typeof(BlogController);

        await Assert.That(controllerType.GetMethod(nameof(BlogController.GetBlogsForManagement))!
            .GetCustomAttribute<OutputCacheAttribute>()).IsNull();
        await Assert.That(controllerType.GetMethod(nameof(BlogController.GetBlogByIdForManagement))!
            .GetCustomAttribute<OutputCacheAttribute>()).IsNull();
    }

    [Test]
    public async Task GetPublishedBlogs_ShouldDispatchWithAmbientTenantId()
    {
        var tenantId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        var assembler = Substitute.For<IResourceAssembler<BlogDetailDto, BlogSummaryDto>>();
        var page = PaginatedResult<BlogSummaryDto>.Create([], 0, 3, 7);
        var hal = new HalCollectionResource<BlogSummaryDto>();
        mediator.Send(Arg.Any<GetPublishedBlogsQuery>(), Arg.Any<CancellationToken>()).Returns(page);
        assembler.ToCollectionResource(page, RouteNames.GetPublishedBlogs, Arg.Any<object?>(), Arg.Any<HttpContext>())
            .Returns(hal);

        var controller = CreateController(tenantId, mediator, assembler);
        await controller.GetPublishedBlogs(new PaginationQueryRequest { PageNumber = 3, PageSize = 7 }, search: "zakat");

        await mediator.Received(1).Send(
            Arg.Is<GetPublishedBlogsQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.Page == 3 &&
                query.PageSize == 7 &&
                query.Search == "zakat"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetPublishedBlogBySlug_WhenMissing_ShouldDispatchWithAmbientTenantIdAndReturnNotFound()
    {
        var tenantId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetPublishedBlogBySlugQuery>(), Arg.Any<CancellationToken>()).Returns((BlogDetailDto?)null);

        var result = await CreateController(tenantId, mediator).GetPublishedBlogBySlug("missing-blog-post");

        await mediator.Received(1).Send(
            Arg.Is<GetPublishedBlogBySlugQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.Slug == "missing-blog-post"),
            Arg.Any<CancellationToken>());
        var objectResult = result.Result as ObjectResult;
        await Assert.That(objectResult).IsNotNull();
        await Assert.That(objectResult!.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task GetBlogsForManagement_ShouldDispatchWithAmbientTenantId()
    {
        var tenantId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        var assembler = Substitute.For<IResourceAssembler<BlogDetailDto, BlogSummaryDto>>();
        var page = PaginatedResult<BlogSummaryDto>.Create([], 0, 2, 5);
        mediator.Send(Arg.Any<GetBlogsForManagementQuery>(), Arg.Any<CancellationToken>()).Returns(page);
        assembler.ToCollectionResource(page, RouteNames.GetBlogsForManagement, Arg.Any<object?>(), Arg.Any<HttpContext>())
            .Returns(new HalCollectionResource<BlogSummaryDto>());

        await CreateController(tenantId, mediator, assembler)
            .GetBlogsForManagement(new PaginationQueryRequest { PageNumber = 2, PageSize = 5 }, "draft");

        await mediator.Received(1).Send(
            Arg.Is<GetBlogsForManagementQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.Page == 2 &&
                query.PageSize == 5 &&
                query.Search == "draft"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetBlogByIdForManagement_WhenMissing_ShouldDispatchWithAmbientTenantIdAndReturnNotFound()
    {
        var tenantId = Guid.CreateVersion7();
        var blogId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetBlogByIdForManagementQuery>(), Arg.Any<CancellationToken>()).Returns((BlogDetailDto?)null);

        var result = await CreateController(tenantId, mediator).GetBlogByIdForManagement(blogId);

        await mediator.Received(1).Send(
            Arg.Is<GetBlogByIdForManagementQuery>(query =>
                query != null &&
                query.TenantId == tenantId &&
                query.BlogId == blogId),
            Arg.Any<CancellationToken>());
        var objectResult = result.Result as ObjectResult;
        await Assert.That(objectResult).IsNotNull();
        await Assert.That(objectResult!.StatusCode).IsEqualTo(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task Create_ShouldDispatchWithAmbientTenantIdAndRedirectToManagementDetailRoute()
    {
        var tenantId = Guid.CreateVersion7();
        var blogId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateBlogCommand>(), Arg.Any<CancellationToken>())
            .Returns(new BaseCommandResponse<Guid> { Success = true, Id = blogId });

        var result = await CreateController(tenantId, mediator).Create(new CreateBlogDto
        {
            Title = "Draft post",
            ContentMarkdown = "# Draft"
        });

        await mediator.Received(1).Send(
            Arg.Is<CreateBlogCommand>(command => command != null && command.TenantId == tenantId),
            Arg.Any<CancellationToken>());
        var created = result.Result as CreatedAtRouteResult;
        await Assert.That(created).IsNotNull();
        await Assert.That(created!.RouteName).IsEqualTo(RouteNames.GetBlogByIdForManagement);
        await Assert.That(created.RouteValues!["id"]).IsEqualTo(blogId);
    }

    [Test]
    public async Task WriteActions_ShouldDispatchWithAmbientTenantId()
    {
        var tenantId = Guid.CreateVersion7();
        var blogId = Guid.CreateVersion7();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateBlogCommand>(), Arg.Any<CancellationToken>())
            .Returns(new BaseCommandResponse<Guid> { Success = true, Id = blogId });
        mediator.Send(Arg.Any<PublishBlogCommand>(), Arg.Any<CancellationToken>())
            .Returns(new BaseCommandResponse<Guid> { Success = true, Id = blogId });
        mediator.Send(Arg.Any<UnpublishBlogCommand>(), Arg.Any<CancellationToken>())
            .Returns(new BaseCommandResponse<Guid> { Success = true, Id = blogId });
        mediator.Send(Arg.Any<DeleteBlogCommand>(), Arg.Any<CancellationToken>())
            .Returns(new BaseCommandResponse<Guid> { Success = true, Id = blogId });
        var controller = CreateController(tenantId, mediator);

        await controller.Update(blogId, new UpdateBlogDto { Title = "Updated", ContentMarkdown = "# Updated" });
        await controller.Publish(blogId);
        await controller.Unpublish(blogId);
        await controller.Delete(blogId);

        await mediator.Received(1).Send(
            Arg.Is<UpdateBlogCommand>(command => command != null && command.TenantId == tenantId && command.BlogId == blogId),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<PublishBlogCommand>(command => command != null && command.TenantId == tenantId && command.BlogId == blogId),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<UnpublishBlogCommand>(command => command != null && command.TenantId == tenantId && command.BlogId == blogId),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<DeleteBlogCommand>(command => command != null && command.TenantId == tenantId && command.BlogId == blogId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetPublishedBlogs_ShouldReturnOk_WithPaginatedResult()
    {
        var response = await _fixture.Client.GetAsync(BaseUrl);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).Contains("items");
    }

    [Test]
    [Arguments(1, 10)]
    [Arguments(1, 20)]
    [Arguments(2, 5)]
    public async Task GetPublishedBlogs_WithPaginationParams_ShouldReturnPaginatedResult(int pageNumber, int pageSize)
    {
        var response = await _fixture.Client.GetAsync($"{BaseUrl}?pageNumber={pageNumber}&pageSize={pageSize}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPublishedBlogBySlug_WithUnknownSlug_ShouldReturnNotFound()
    {
        var response = await _fixture.Client.GetAsync($"{BaseUrl}/missing-blog-post");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetBlogsForManagement_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _fixture.Client.GetAsync($"{BaseUrl}/manage");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetBlogByIdForManagement_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _fixture.Client.GetAsync($"{BaseUrl}/manage/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Create_WithoutAuth_ShouldReturnUnauthorized()
    {
        var createDto = new CreateBlogDto
        {
            Title = "Draft post",
            ContentMarkdown = "# Draft"
        };

        var response = await _fixture.Client.PostAsJsonAsync(BaseUrl, createDto);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Update_WithoutAuth_ShouldReturnUnauthorized()
    {
        var updateDto = new UpdateBlogDto
        {
            Title = "Updated post",
            ContentMarkdown = "# Updated"
        };

        var response = await _fixture.Client.PatchAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}", updateDto);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Publish_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _fixture.Client.PostAsync($"{BaseUrl}/{Guid.NewGuid()}/publish", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Unpublish_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _fixture.Client.PostAsync($"{BaseUrl}/{Guid.NewGuid()}/unpublish", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Delete_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _fixture.Client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    private static BlogController CreateController(
        Guid tenantId,
        IMediator mediator,
        IResourceAssembler<BlogDetailDto, BlogSummaryDto>? assembler = null)
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(tenantId);
        return new BlogController(
            mediator,
            tenantContext,
            assembler ?? Substitute.For<IResourceAssembler<BlogDetailDto, BlogSummaryDto>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
