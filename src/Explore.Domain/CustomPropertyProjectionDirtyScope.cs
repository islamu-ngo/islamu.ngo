// ABOUTME: EF Core entity for custom-property projection dirty-scope backlog.
// ABOUTME: Generic ITenantEntity with ProjectionName, Version, ScopeType, ScopeId, Reason, CreatedAt, DrainedAt.

namespace Explore.Domain;

public class CustomPropertyProjectionDirtyScope : ITenantEntity
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string ProjectionName { get; set; } = string.Empty;
    public int ProjectionVersion { get; set; }
    public CustomPropertyProjectionScopeType ScopeType { get; set; }
    public Guid ScopeId { get; set; }
    public Guid? DefinitionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DrainedAt { get; set; }
}
