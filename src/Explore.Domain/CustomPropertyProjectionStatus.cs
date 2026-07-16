// ABOUTME: EF Core entity for custom-property projection rebuild status.
// ABOUTME: Composite-key entity with ProjectionName, Version, TenantId, State, rebuild counters, checkpoint, error.

namespace Explore.Domain;

public class CustomPropertyProjectionStatus : ITenantEntity
{
    public string ProjectionName { get; set; } = string.Empty;
    public int ProjectionVersion { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public CustomPropertyProjectionState State { get; set; }
    public DateTime? LastRebuildStartedAt { get; set; }
    public DateTime? LastRebuildCompletedAt { get; set; }
    public long RowsProcessed { get; set; }
    public long RowsFailed { get; set; }
    public string? LastCheckpoint { get; set; }
    public string? LastErrorMessage { get; set; }
}
