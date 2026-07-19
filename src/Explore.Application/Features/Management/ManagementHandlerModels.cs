// ABOUTME: Defines internal data holders shared by managed registration and tenant provisioning handlers.
// ABOUTME: Keeps non-handler implementation types outside CQRS handler namespaces.

using Explore.Application.DTOs.Management;
using Explore.Application.Responses;
using Explore.Domain;

namespace Explore.Application.Features.Management;

internal sealed record ReplayResolution(
    BaseCommandResponse<ManagementTenantProvisioningOperationDto>? ImmediateResponse,
    ManagedTenantProvisioningOperation? RetryCandidate);

internal sealed record RegistrationSecrets(
    string EventToControlPlaneSecret,
    string? ControlPlaneToEventSecret);

internal enum CommittedRecoveryResolution
{
    None,
    Owned,
    Conflict
}
