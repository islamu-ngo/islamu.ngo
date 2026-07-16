// ABOUTME: EF Core entity for tenant-scoped organization reviews.
// ABOUTME: Retains organization, reviewer, rating, comment, audit, and soft-delete persistence without product-specific relationships.

using Explore.Domain.Interfaces;

namespace Explore.Domain;

public class OrganizationReview : ITenantEntity, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string? ReviewerName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
