// ABOUTME: Default catalog of platform-owned scheduled jobs.
// ABOUTME: Separates implemented scheduler work from planned migrations while preserving stable job names.

using Explore.Application.Contracts.Scheduling;

namespace Explore.Application.Services;

public sealed class ScheduledJobRegistry : IScheduledJobRegistry
{
    private static readonly ScheduledJobDescriptor[] Jobs =
    [
        new(
            ScheduledJobNames.EmailDispatchDrain,
            "EmailDispatch",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Implemented,
            "Claims due EmailDispatchOutbox rows and executes approved dispatch transports.",
            "*/10 * * * * *"),
        new(
            ScheduledJobNames.GeneralOutboxDrain,
            "Outbox",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Planned,
            "Future drain for general integration outbox work after EmailDispatch proves multi-node behavior."),
        new(
            ScheduledJobNames.PdsSyncDrain,
            "PDS",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Planned,
            "Future AT Protocol PDS sync drain using existing durable PDS state."),
        new(
            ScheduledJobNames.EmailDispatchRecoveryScan,
            "EmailDispatch",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Implemented,
            "Marks stale EmailDispatchOutbox processing leases as Unknown for operator review.",
            "0 */1 * * * *"),
        new(
            ScheduledJobNames.DeadLetterSummary,
            "Operations",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Planned,
            "Future operator summary for dead-lettered platform work."),
        new(
            ScheduledJobNames.TenantMaintenanceScan,
            "Operations",
            ScheduledJobScheduleKind.Cron,
            ScheduledJobPayloadKind.None,
            ScheduledJobOperationalStatus.Planned,
            "Future tenant maintenance scanner for scheduled platform controls.")
    ];

    public IReadOnlyCollection<ScheduledJobDescriptor> ListJobs()
    {
        return Jobs;
    }

    public ScheduledJobDescriptor? FindByName(string name)
    {
        return Jobs.FirstOrDefault(job => string.Equals(job.Name, name, StringComparison.Ordinal));
    }
}
