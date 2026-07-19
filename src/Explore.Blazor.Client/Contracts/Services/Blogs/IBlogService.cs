// ABOUTME: Client contract for public and editorial Blog operations through the generated API client.
// ABOUTME: Keeps Blog components dependent on an interface while preserving generated HAL response types.

using Explore.Blazor.Client.Clients;

namespace Explore.Blazor.Client.Contracts.Services.Blogs;

public interface IBlogService
{
    Task<HalCollectionResourceOfBlogSummaryDto> GetPublishedBlogsAsync(
        int? pageNumber = null,
        int? pageSize = null,
        Guid? categoryId = null,
        Guid? tagId = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<BlogDetailDto?> GetPublishedBlogBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<HalCollectionResourceOfBlogSummaryDto> GetBlogsForManagementAsync(
        int? pageNumber = null,
        int? pageSize = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<HalResourceOfBlogDetailDto?> GetBlogByIdForManagementAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid?> CreateBlogAsync(
        CreateBlogDto dto,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid?> UpdateBlogAsync(
        Guid id,
        UpdateBlogDto dto,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid?> PublishBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BaseCommandResponseOfGuid?> UnpublishBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
