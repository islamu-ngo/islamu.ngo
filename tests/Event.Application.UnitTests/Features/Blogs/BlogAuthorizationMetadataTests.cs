// ABOUTME: Unit tests for Blog request authorization metadata and DTO resource descriptors.
// ABOUTME: Guards the Blog Application authorization contract without invoking handlers or external policy engines.

using System.Reflection;
using Explore.Application.Authorization;
using Explore.Application.DTOs.Blog;
using Explore.Application.Features.Blogs.Requests;
using Explore.Application.Features.Blogs.Requests.Commands;

namespace Event.Application.UnitTests.Features.Blogs;

public sealed class BlogAuthorizationMetadataTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid BlogId = Guid.NewGuid();
    private static readonly DateTime PublishedAtUtc = new(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

    public static IEnumerable<(string Constant, string Expected)> BlogConstants()
    {
        yield return (ResourceKinds.Blog, "islamuevent_blog");
        yield return (AuthorizationActions.Blogs.View, AuthorizationActions.View);
        yield return (AuthorizationActions.Blogs.Create, AuthorizationActions.Create);
        yield return (AuthorizationActions.Blogs.Update, AuthorizationActions.Update);
        yield return (AuthorizationActions.Blogs.Delete, AuthorizationActions.Delete);
        yield return (AuthorizationActions.Blogs.Publish, "publish");
        yield return (AuthorizationActions.Blogs.Unpublish, "unpublish");
    }

    [Test]
    [MethodDataSource(nameof(BlogConstants))]
    public async Task BlogAuthorizationConstantsMatchPolicyContract((string Constant, string Expected) testCase)
    {
        await Assert.That(testCase.Constant).IsEqualTo(testCase.Expected);
    }

    public static IEnumerable<(Type RequestType, string ExpectedAction)> BlogWriteRequests()
    {
        yield return (typeof(CreateBlogCommand), AuthorizationActions.Blogs.Create);
        yield return (typeof(UpdateBlogCommand), AuthorizationActions.Blogs.Update);
        yield return (typeof(PublishBlogCommand), AuthorizationActions.Blogs.Publish);
        yield return (typeof(UnpublishBlogCommand), AuthorizationActions.Blogs.Unpublish);
        yield return (typeof(DeleteBlogCommand), AuthorizationActions.Blogs.Delete);
    }

    [Test]
    [MethodDataSource(nameof(BlogWriteRequests))]
    public async Task BlogWriteRequestsRequireBlogAuthorization((Type RequestType, string ExpectedAction) testCase)
    {
        var attribute = testCase.RequestType.GetCustomAttribute<AuthorizeResourceAttribute>();

        await Assert.That(attribute).IsNotNull();
        await Assert.That(attribute!.Resource).IsEqualTo(ResourceKinds.Blog);
        await Assert.That(attribute.Action).IsEqualTo(testCase.ExpectedAction);
        await Assert.That(typeof(ISecureRequest).IsAssignableFrom(testCase.RequestType)).IsTrue();
    }

    public static IEnumerable<Type> BlogReadRequests()
    {
        yield return typeof(GetPublishedBlogsQuery);
        yield return typeof(GetPublishedBlogBySlugQuery);
    }

    [Test]
    [MethodDataSource(nameof(BlogReadRequests))]
    public async Task BlogPublicReadRequestsRemainUndecorated(Type requestType)
    {
        await Assert.That(requestType.GetCustomAttribute<AuthorizeResourceAttribute>()).IsNull();
        await Assert.That(typeof(ISecureRequest).IsAssignableFrom(requestType)).IsFalse();
    }

    public static IEnumerable<Type> BlogManagementReadRequests()
    {
        yield return typeof(GetBlogsForManagementQuery);
        yield return typeof(GetBlogByIdForManagementQuery);
    }

    [Test]
    [MethodDataSource(nameof(BlogManagementReadRequests))]
    public async Task BlogManagementReadRequestsRequireViewAuthorization(Type requestType)
    {
        var attribute = requestType.GetCustomAttribute<AuthorizeResourceAttribute>();

        await Assert.That(attribute).IsNotNull();
        await Assert.That(attribute!.Resource).IsEqualTo(ResourceKinds.Blog);
        await Assert.That(attribute.Action).IsEqualTo(AuthorizationActions.Blogs.View);
        await Assert.That(typeof(ISecureRequest).IsAssignableFrom(requestType)).IsTrue();
    }

    public static IEnumerable<(ISecureRequest Request, string ExpectedResourceId, string? ExpectedPublicationIntent)> AuthorizedBlogRequests()
    {
        yield return (new CreateBlogCommand(TenantId, new CreateBlogDto()), TenantId.ToString("D"), null);
        yield return (new UpdateBlogCommand(TenantId, BlogId, new UpdateBlogDto()), BlogId.ToString("D"), null);
        yield return (new PublishBlogCommand(TenantId, BlogId), BlogId.ToString("D"), AuthorizationActions.Blogs.Publish);
        yield return (new UnpublishBlogCommand(TenantId, BlogId), BlogId.ToString("D"), AuthorizationActions.Blogs.Unpublish);
        yield return (new DeleteBlogCommand(TenantId, BlogId), BlogId.ToString("D"), null);
    }

    [Test]
    [MethodDataSource(nameof(AuthorizedBlogRequests))]
    public async Task BlogWriteRequestsExposeTenantAndAggregateAuthorizationContext(
        (ISecureRequest Request, string ExpectedResourceId, string? ExpectedPublicationIntent) testCase)
    {
        await Assert.That(testCase.Request.ResourceId).IsEqualTo(testCase.ExpectedResourceId);
        await Assert.That(testCase.Request.ResourceAttributes).IsNotNull();
        await Assert.That(testCase.Request.ResourceAttributes!["tenantId"]).IsEqualTo(TenantId.ToString("D"));

        if (testCase.Request is not CreateBlogCommand)
        {
            await Assert.That(testCase.Request.ResourceAttributes["blogId"]).IsEqualTo(BlogId.ToString("D"));
        }

        if (testCase.ExpectedPublicationIntent is not null)
        {
            await Assert.That(testCase.Request.ResourceAttributes["publicationIntent"]).IsEqualTo(testCase.ExpectedPublicationIntent);
        }
    }

    [Test]
    public async Task BlogManagementReadRequestsExposeTenantAndAvailableBlogAuthorizationContext()
    {
        ISecureRequest listRequest = new GetBlogsForManagementQuery(TenantId);
        ISecureRequest detailRequest = new GetBlogByIdForManagementQuery(TenantId, BlogId);

        await Assert.That(listRequest.ResourceId).IsEqualTo(TenantId.ToString("D"));
        await Assert.That(listRequest.ResourceAttributes).IsNotNull();
        await Assert.That(listRequest.ResourceAttributes!["tenantId"]).IsEqualTo(TenantId.ToString("D"));
        await Assert.That(listRequest.ResourceAttributes.Keys).DoesNotContain("blogId");
        await Assert.That(detailRequest.ResourceId).IsEqualTo(BlogId.ToString("D"));
        await Assert.That(detailRequest.ResourceAttributes).IsNotNull();
        await Assert.That(detailRequest.ResourceAttributes!["tenantId"]).IsEqualTo(TenantId.ToString("D"));
        await Assert.That(detailRequest.ResourceAttributes["blogId"]).IsEqualTo(BlogId.ToString("D"));
    }

    public static IEnumerable<ISecureRequest> EmptyIdentifierBlogRequests()
    {
        yield return new GetBlogsForManagementQuery(Guid.Empty);
        yield return new GetBlogByIdForManagementQuery(TenantId, Guid.Empty);
        yield return new GetBlogByIdForManagementQuery(Guid.Empty, BlogId);
        yield return new CreateBlogCommand(Guid.Empty, new CreateBlogDto());
        yield return new UpdateBlogCommand(TenantId, Guid.Empty, new UpdateBlogDto());
        yield return new UpdateBlogCommand(Guid.Empty, BlogId, new UpdateBlogDto());
        yield return new PublishBlogCommand(TenantId, Guid.Empty);
        yield return new UnpublishBlogCommand(Guid.Empty, BlogId);
        yield return new DeleteBlogCommand(Guid.Empty, BlogId);
    }

    [Test]
    [MethodDataSource(nameof(EmptyIdentifierBlogRequests))]
    public async Task BlogSecureRequestsFailClosedWhenTenantOrAggregateIdsAreEmpty(ISecureRequest request)
    {
        await Assert.That(request.ResourceAttributes).IsNull();
    }

    public static IEnumerable<(Type DtoType, string ExpectedResourceKind)> BlogDtoTypes()
    {
        yield return (typeof(BlogSummaryDto), ResourceKinds.Blog);
        yield return (typeof(BlogDetailDto), ResourceKinds.Blog);
    }

    [Test]
    [MethodDataSource(nameof(BlogDtoTypes))]
    public async Task BlogDtosResolveToBlogResourceKind((Type DtoType, string ExpectedResourceKind) testCase)
    {
        await Assert.That(ResourceDescriptorRegistry.ResolveResourceKind(testCase.DtoType))
            .IsEqualTo(testCase.ExpectedResourceKind);
    }

    [Test]
    public async Task BlogSummaryDescriptorExposesTenantAndPublicationMetadata()
    {
        var dto = BlogSummary();
        var attributes = ResourceDescriptors.BlogSummary.GetResourceAttributes(dto);
        var scope = ResourceDescriptors.BlogSummary.GetScope(dto);

        await Assert.That(ResourceDescriptors.BlogSummary.Kind).IsEqualTo(ResourceKinds.Blog);
        await Assert.That(ResourceDescriptors.BlogSummary.GetResourceId(dto)).IsEqualTo(BlogId.ToString());
        await Assert.That(attributes["blogId"]).IsEqualTo(BlogId.ToString());
        await Assert.That(attributes["tenantId"]).IsEqualTo(TenantId.ToString());
        await Assert.That(attributes["isPublished"]).IsEqualTo(true);
        await Assert.That(attributes["hasPublishedAt"]).IsEqualTo(true);
        await Assert.That(attributes["publishedAtUtc"]).IsEqualTo(PublishedAtUtc);
        await Assert.That(scope.TenantId).IsEqualTo(TenantId.ToString());
    }

    [Test]
    public async Task BlogDetailDescriptorOmitsPublishedAtWhenUnknown()
    {
        var dto = BlogDetail();
        var attributes = ResourceDescriptors.BlogDetail.GetResourceAttributes(dto);

        await Assert.That(ResourceDescriptors.BlogDetail.Kind).IsEqualTo(ResourceKinds.Blog);
        await Assert.That(ResourceDescriptors.BlogDetail.GetResourceId(dto)).IsEqualTo(BlogId.ToString());
        await Assert.That(attributes["blogId"]).IsEqualTo(BlogId.ToString());
        await Assert.That(attributes["tenantId"]).IsEqualTo(TenantId.ToString());
        await Assert.That(attributes["isPublished"]).IsEqualTo(false);
        await Assert.That(attributes["hasPublishedAt"]).IsEqualTo(false);
        await Assert.That(attributes.Keys).DoesNotContain("publishedAtUtc");
        await Assert.That(ResourceDescriptors.BlogDetail.GetScope(dto).TenantId).IsEqualTo(TenantId.ToString());
    }

    private static BlogSummaryDto BlogSummary() => new()
    {
        Id = BlogId,
        TenantId = TenantId,
        PublishedAtUtc = PublishedAtUtc
    };

    private static BlogDetailDto BlogDetail() => new()
    {
        Id = BlogId,
        TenantId = TenantId
    };
}
