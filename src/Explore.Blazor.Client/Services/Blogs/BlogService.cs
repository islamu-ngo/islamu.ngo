// ABOUTME: Service for blog operations using the generated IIslamuApiClient.
// ABOUTME: Provides public and management blog queries and commands with safe fallbacks.

using Explore.Blazor.Client.Clients;
using Explore.Blazor.Client.Helpers;
using Microsoft.Extensions.Logging;

namespace Explore.Blazor.Client.Services.Blogs;

public sealed class BlogService
{
    private readonly IIslamuApiClient _apiClient;
    private readonly ILogger<BlogService> _logger;

    public BlogService(IIslamuApiClient apiClient, ILogger<BlogService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HalCollectionResourceOfBlogSummaryDto> GetPublishedBlogsAsync(
        int? pageNumber = null,
        int? pageSize = null,
        Guid? categoryId = null,
        Guid? tagId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetPublishedBlogsAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                categoryId: categoryId,
                tagId: tagId,
                search: search,
                cancellationToken: cancellationToken);

            return result ?? new HalCollectionResourceOfBlogSummaryDto();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error fetching published blogs: {StatusCode}", ex.StatusCode);
            return new HalCollectionResourceOfBlogSummaryDto();
        }
    }

    public async Task<BlogDetailDto?> GetPublishedBlogBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetPublishedBlogBySlugAsync(
                slug: slug,
                cancellationToken: cancellationToken);

            return result?.ToDto();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("[BLOG SERVICE] Published blog not found for slug: {Slug}", slug);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error fetching published blog by slug {Slug}: {StatusCode}", slug, ex.StatusCode);
            return null;
        }
    }

    public async Task<HalCollectionResourceOfBlogSummaryDto> GetBlogsForManagementAsync(
        int? pageNumber = null,
        int? pageSize = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetBlogsForManagementAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                search: search,
                cancellationToken: cancellationToken);

            return result ?? new HalCollectionResourceOfBlogSummaryDto();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error fetching management blogs: {StatusCode}", ex.StatusCode);
            return new HalCollectionResourceOfBlogSummaryDto();
        }
    }

    public async Task<HalResourceOfBlogDetailDto?> GetBlogByIdForManagementAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetBlogByIdForManagementAsync(
                id: id,
                cancellationToken: cancellationToken);

            return result;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            _logger.LogWarning("[BLOG SERVICE] Management blog not found for ID: {BlogId}", id);
            return null;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error fetching management blog by ID {BlogId}: {StatusCode}", id, ex.StatusCode);
            return null;
        }
    }

    public async Task<BaseCommandResponseOfGuid?> CreateBlogAsync(
        CreateBlogDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.CreateBlogAsync(
                body: dto,
                cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error creating blog: {StatusCode}", ex.StatusCode);
            return new BaseCommandResponseOfGuid
            {
                Success = false,
                Message = $"API error: {ex.Message}",
                Errors = new List<string> { ex.Response ?? ex.Message }
            };
        }
    }

    public async Task<BaseCommandResponseOfGuid?> UpdateBlogAsync(
        Guid id,
        UpdateBlogDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.UpdateBlogAsync(
                id: id,
                body: dto,
                cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error updating blog: {StatusCode}", ex.StatusCode);
            return new BaseCommandResponseOfGuid
            {
                Success = false,
                Message = $"API error: {ex.Message}",
                Errors = new List<string> { ex.Response ?? ex.Message }
            };
        }
    }

    public async Task<BaseCommandResponseOfGuid?> PublishBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.PublishBlogAsync(
                id: id,
                cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error publishing blog: {StatusCode}", ex.StatusCode);
            return new BaseCommandResponseOfGuid
            {
                Success = false,
                Message = $"API error: {ex.Message}",
                Errors = new List<string> { ex.Response ?? ex.Message }
            };
        }
    }

    public async Task<BaseCommandResponseOfGuid?> UnpublishBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.UnpublishBlogAsync(
                id: id,
                cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error unpublishing blog: {StatusCode}", ex.StatusCode);
            return new BaseCommandResponseOfGuid
            {
                Success = false,
                Message = $"API error: {ex.Message}",
                Errors = new List<string> { ex.Response ?? ex.Message }
            };
        }
    }

    public async Task<bool> DeleteBlogAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.DeleteBlogAsync(
                id: id,
                cancellationToken: cancellationToken);
            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "[BLOG SERVICE] API error deleting blog: {StatusCode}", ex.StatusCode);
            return false;
        }
    }
}
