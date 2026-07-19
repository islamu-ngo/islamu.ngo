// ABOUTME: Application persistence contract for generic organization review reads.
// ABOUTME: Keeps organization and reviewer queries independent of removed product relationships.

using Explore.Domain;

namespace Explore.Application.Contracts.Persistence;

public interface IOrganizationReviewRepository : IGenericRepository<OrganizationReview, Guid>
{
    Task<List<OrganizationReview>> GetByOrganizationId(Guid organizationId);
    Task<List<OrganizationReview>> GetByUserId(Guid userId);
}
