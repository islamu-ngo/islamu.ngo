// ABOUTME: Reads one managed tenant provisioning operation without mutating its durable state.
// ABOUTME: Maps the tenant-scoped operation entity to its management DTO in the query handler.

using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Management;
using Explore.Application.Features.Management.Requests.Queries;
using Explore.Domain;
using MediatR;

namespace Explore.Application.Features.Management.Handlers.Queries;

public sealed class GetManagedTenantProvisioningOperationQueryHandler(
    IManagedTenantProvisioningOperationRepository operationRepository)
    : IRequestHandler<GetManagedTenantProvisioningOperationQuery,
        ManagementTenantProvisioningOperationDto?>
{
    public async Task<ManagementTenantProvisioningOperationDto?> Handle(
        GetManagedTenantProvisioningOperationQuery request,
        CancellationToken cancellationToken)
    {
        ManagedTenantProvisioningOperation? operation =
            await operationRepository.GetByManagedInstanceAndIdAsNoTrackingAsync(
                request.ManagedInstanceId,
                request.OperationId,
                cancellationToken);
        return operation is null ? null : ManagedTenantProvisioningRequestCodec.ToDto(operation);
    }
}
