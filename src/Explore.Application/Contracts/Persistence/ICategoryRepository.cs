// ABOUTME: Repository contract for category lookup and detail queries.
// ABOUTME: Returns category entities so application handlers keep mapping ownership.

using Explore.Domain;

namespace Explore.Application.Contracts.Persistence;

public interface ICategoryRepository : IGenericRepository<Category, Guid>
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Category> GetCategoryWithDetails(Guid id);
    Task<List<Category>> GetCategoriesWithDetails();
    Task<(List<Category> Items, int TotalCount)> GetCategoriesWithDetailsPaged(int pageNumber, int pageSize);
}
