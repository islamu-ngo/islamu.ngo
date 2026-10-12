// ABOUTME: Provides entity-first persistence for the retained Madhab lookup table.
// ABOUTME: Reuses the generic EF Core repository so Application handlers own DTO mapping.

using Explore.Application.Contracts.Persistence;
using Explore.Domain;

namespace Explore.Persistence.Repositories;

public sealed class MadhabRepository(ExploreDbContext dbContext)
    : GenericRepository<Madhab, int>(dbContext), IMadhabRepository
{
}
