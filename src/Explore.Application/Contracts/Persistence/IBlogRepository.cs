// ABOUTME: Repository contract for tenant-scoped blog aggregate persistence and reads.
// ABOUTME: Returns Blog entities only and keeps query shaping behind cancellation-aware methods.

using Explore.Application.Specifications;
using Explore.Domain;

namespace Explore.Application.Contracts.Persistence;

public interface IBlogRepository
{
    Task<Blog> CreateAsync(Blog blog, CancellationToken cancellationToken);

    Task<Blog> UpdateAsync(Blog blog, CancellationToken cancellationToken);

    Task<Blog?> GetByTenantAndIdAsync(
        Guid tenantId,
        Guid blogId,
        CancellationToken cancellationToken);

    Task<Blog?> GetByTenantAndSlugAsync(
        Guid tenantId,
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Blog>> ListAsync(
        BlogQuerySpecification specification,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(
        BlogQuerySpecification specification,
        CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(
        Guid tenantId,
        string slug,
        Guid? excludingBlogId,
        CancellationToken cancellationToken);
}
