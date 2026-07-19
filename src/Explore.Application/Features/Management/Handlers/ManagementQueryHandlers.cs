// ABOUTME: Projects instance-owned deployment mode, version, registration, and capability metadata for managed mode.
// ABOUTME: Returns only bounded instance lifecycle data and keeps standalone mode absent by default.

using Explore.Application.Contracts.Infrastructure;
using Explore.Application.Contracts.Persistence;
using Explore.Application.Contracts.Services;
using Explore.Application.DTOs.Management;
using Explore.Application.Features.Management;
using Explore.Application.Features.Management.Requests.Queries;
using Explore.Application.Management;
using Explore.Domain;
using Explore.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Options;

namespace Explore.Application.Features.Management.Handlers.Queries;

public sealed class GetManagementCapabilitiesQueryHandler(
    IOptions<ManagedControlPlaneOptions> options,
    IInstanceBootstrapStateRepository bootstrapStateRepository,
    IManagedControlPlaneRegistrationRepository registrationRepository,
    ManagedTenantProvisioningCapacityReader tenantProvisioningCapacityReader)
    : IRequestHandler<GetManagementCapabilitiesQuery, ManagementCapabilitiesDto>
{
    public async Task<ManagementCapabilitiesDto> Handle(
        GetManagementCapabilitiesQuery request,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return new ManagementCapabilitiesDto(
                false,
                ManagedControlPlaneContract.ManagementApiVersion,
                ManagementVersionResolver.EventVersion,
                DeploymentMode.SingleTenant,
                null,
                "Disabled",
                [],
                null);
        }

        var bootstrap = await bootstrapStateRepository.GetCurrent(cancellationToken);
        var registration = await registrationRepository.GetCurrentAsync(cancellationToken);
        DeploymentMode mode = bootstrap?.IsCompleted == true
            && Enum.TryParse(bootstrap.SelectedDeploymentMode, out DeploymentMode persistedMode)
                ? persistedMode
                : DeploymentMode.SingleTenant;
        ManagementTenantProvisioningCapacityDto? capacity = mode == DeploymentMode.MultiTenant
            && options.Value.MaximumTenantCount > 0
                ? await tenantProvisioningCapacityReader.ReadAsync(cancellationToken, mode)
                : null;
        IReadOnlyList<string> capabilities = capacity is not null
                ? ManagedControlPlaneContract.Capabilities
                    .Concat(ManagedControlPlaneContract.TenantProvisioningCapabilities)
                    .ToArray()
                : ManagedControlPlaneContract.Capabilities;

        return new ManagementCapabilitiesDto(
            true,
            ManagedControlPlaneContract.ManagementApiVersion,
            ManagementVersionResolver.EventVersion,
            mode,
            bootstrap is { IsCompleted: true } ? bootstrap.Id : null,
            registration?.Status.ToString() ?? "Unregistered",
            capabilities,
            capacity);
    }
}

public sealed class GetManagedEventInstanceStatusQueryHandler(
    IOptions<ManagedControlPlaneOptions> options,
    IDeploymentModeProvider deploymentModeProvider,
    IManagedControlPlaneRegistrationRepository registrationRepository)
    : IRequestHandler<GetManagedEventInstanceStatusQuery, ManagedEventInstanceStatusDto?>
{
    public async Task<ManagedEventInstanceStatusDto?> Handle(
        GetManagedEventInstanceStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return null;
        }

        var registration = await registrationRepository.GetCurrentAsync(cancellationToken);
        if (registration is null)
        {
            return null;
        }

        var mode = await deploymentModeProvider.GetCurrentModeAsync(cancellationToken);

        return new ManagedEventInstanceStatusDto(
            registration.ManagedInstanceId,
            registration.EventInstanceId,
            mode,
            ManagementVersionResolver.EventVersion,
            ManagedControlPlaneContract.ManagementApiVersion,
            registration.Status.ToString(),
            registration.RegisteredAt,
            registration.EventToControlPlaneCredentialExpiresAt,
            registration.ControlPlaneToEventCredentialExpiresAt);
    }
}

public sealed class GetManagementHealthQueryHandler(IManagedEventHealthProbe healthProbe)
    : IRequestHandler<GetManagementHealthQuery, ManagementHealthDto>
{
    public async Task<ManagementHealthDto> Handle(
        GetManagementHealthQuery request,
        CancellationToken cancellationToken)
    {
        var health = await healthProbe.CheckAsync(cancellationToken);
        return new ManagementHealthDto(health.Status, health.ObservedAt);
    }
}

public sealed class GetManagementUpgradePreflightQueryHandler(
    IOptions<ManagedControlPlaneOptions> options,
    IDeploymentModeProvider deploymentModeProvider,
    IInstanceBootstrapStateRepository bootstrapStateRepository,
    IManagedControlPlaneRegistrationRepository registrationRepository,
    IManagedEventHealthProbe healthProbe)
    : IRequestHandler<GetManagementUpgradePreflightQuery, ManagementUpgradePreflightDto>
{
    public async Task<ManagementUpgradePreflightDto> Handle(
        GetManagementUpgradePreflightQuery request,
        CancellationToken cancellationToken)
    {
        var currentVersion = ManagementVersionResolver.EventVersion;
        var targetVersion = request.TargetEventVersion?.Trim() ?? string.Empty;
        var targetApiVersion = request.TargetManagementApiVersion?.Trim() ?? string.Empty;
        var now = DateTime.UtcNow;
        var blockers = new List<ManagementUpgradeBlockerDto>();
        if (!options.Value.Enabled)
        {
            ManagementUpgradeAssessment.AddBlocker(
                blockers,
                "managed_mode_disabled",
                "Managed mode is disabled on this ISLAMU instance.");
            return new ManagementUpgradePreflightDto(
                null,
                null,
                DeploymentMode.SingleTenant,
                currentVersion,
                targetVersion,
                ManagedControlPlaneContract.ManagementApiVersion,
                targetApiVersion,
                "Disabled",
                "Unavailable",
                false,
                blockers,
                now);
        }

        var registration = await registrationRepository.GetCurrentAsync(cancellationToken);
        var bootstrap = await bootstrapStateRepository.GetCurrent(cancellationToken);
        var mode = await deploymentModeProvider.GetCurrentModeAsync(cancellationToken);
        var health = await healthProbe.CheckAsync(cancellationToken);
        ManagementUpgradeAssessment.AddCommonBlockers(
            blockers,
            registration,
            bootstrap is { IsCompleted: true },
            mode,
            health,
            now);
        ManagementUpgradeAssessment.AddPreflightVersionBlockers(
            blockers,
            currentVersion,
            targetVersion,
            targetApiVersion);

        return new ManagementUpgradePreflightDto(
            registration?.ManagedInstanceId,
            registration?.EventInstanceId,
            mode,
            currentVersion,
            targetVersion,
            ManagedControlPlaneContract.ManagementApiVersion,
            targetApiVersion,
            registration?.Status.ToString() ?? "Unregistered",
            health.Status,
            blockers.Count == 0,
            blockers,
            health.ObservedAt);
    }
}

public sealed class GetManagementUpgradePostflightQueryHandler(
    IOptions<ManagedControlPlaneOptions> options,
    IDeploymentModeProvider deploymentModeProvider,
    IInstanceBootstrapStateRepository bootstrapStateRepository,
    IManagedControlPlaneRegistrationRepository registrationRepository,
    IManagedEventHealthProbe healthProbe)
    : IRequestHandler<GetManagementUpgradePostflightQuery, ManagementUpgradePostflightDto>
{
    public async Task<ManagementUpgradePostflightDto> Handle(
        GetManagementUpgradePostflightQuery request,
        CancellationToken cancellationToken)
    {
        var currentVersion = ManagementVersionResolver.EventVersion;
        var expectedVersion = request.ExpectedEventVersion?.Trim() ?? string.Empty;
        var expectedApiVersion = request.ExpectedManagementApiVersion?.Trim() ?? string.Empty;
        var now = DateTime.UtcNow;
        var blockers = new List<ManagementUpgradeBlockerDto>();
        if (!options.Value.Enabled)
        {
            ManagementUpgradeAssessment.AddBlocker(
                blockers,
                "managed_mode_disabled",
                "Managed mode is disabled on this ISLAMU instance.");
            return new ManagementUpgradePostflightDto(
                null,
                null,
                DeploymentMode.SingleTenant,
                currentVersion,
                expectedVersion,
                ManagedControlPlaneContract.ManagementApiVersion,
                expectedApiVersion,
                "Disabled",
                "Unavailable",
                false,
                blockers,
                now);
        }

        var registration = await registrationRepository.GetCurrentAsync(cancellationToken);
        var bootstrap = await bootstrapStateRepository.GetCurrent(cancellationToken);
        var mode = await deploymentModeProvider.GetCurrentModeAsync(cancellationToken);
        var health = await healthProbe.CheckAsync(cancellationToken);
        ManagementUpgradeAssessment.AddCommonBlockers(
            blockers,
            registration,
            bootstrap is { IsCompleted: true },
            mode,
            health,
            now);
        ManagementUpgradeAssessment.AddPostflightVersionBlockers(
            blockers,
            currentVersion,
            expectedVersion,
            expectedApiVersion);

        return new ManagementUpgradePostflightDto(
            registration?.ManagedInstanceId,
            registration?.EventInstanceId,
            mode,
            currentVersion,
            expectedVersion,
            ManagedControlPlaneContract.ManagementApiVersion,
            expectedApiVersion,
            registration?.Status.ToString() ?? "Unregistered",
            health.Status,
            blockers.Count == 0,
            blockers,
            health.ObservedAt);
    }
}
