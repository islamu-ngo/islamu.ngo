// ABOUTME: Repository contract for tag lookup and detail queries.
// ABOUTME: Returns tag entities so application handlers keep mapping ownership.

using Explore.Domain;

namespace Explore.Application.Contracts.Persistence;

public interface ITagRepository : IGenericRepository<Tag, Guid>
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Tag> GetTagWithDetails(Guid id);
    Task<List<Tag>> GetTagsWithDetails();
    Task<(List<Tag> Items, int TotalCount)> GetTagsWithDetailsPaged(int pageNumber, int pageSize);
}
