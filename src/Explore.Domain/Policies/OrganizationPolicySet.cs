// ABOUTME: EF Core entity for organization-level governance policy record.
// ABOUTME: Maps identity, tenancy, audit metadata, concurrency, and organization uniqueness.

using Explore.Domain.Interfaces;

namespace Explore.Domain.Policies;

public class OrganizationPolicySet : ITenantEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
