// ABOUTME: PostgreSQL tests for storage-object deletion retry discovery queries.
// ABOUTME: Verifies delete-requested image rows remain findable by tenant and owning resource after FKs are cleared.

using Event.Persistence.IntegrationTests.Fixtures;
using Explore.Application.Authorization;
using Explore.Domain;
using Explore.Domain.Enums;
using Explore.Persistence.Repositories;
using TUnit.Core;

namespace Event.Persistence.IntegrationTests.Repositories;

[ClassDataSource<PostgreSqlContainerFixture>(Shared = SharedType.PerAssembly)]
[NotInParallel("PersistenceDb")]
public sealed class StorageObjectDeletionRepositoryTests(PostgreSqlContainerFixture fixture)
{
    [Test]
    public async Task ListDeleteRequestedForResourceAsync_ReturnsOnlyMatchingDeleteRequestedRows()
    {
        await fixture.ResetAsync();
        await using var context = fixture.CreateDbContext();
        var tenant = new Tenant
        {
            FullName = "Storage Delete Retry Tenant",
            Slug = "storage-delete-retry-" + Guid.NewGuid().ToString("N")[..8],
            TenantStatusId = (int)TenantStatusEnum.Active,
            TenantStatus = null!
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var resourceId = Guid.CreateVersion7();
        var matching = CreateStorageObject(tenant.Id, resourceId, StorageObjectLifecycleStates.DeleteRequested);
        var missingKey = CreateStorageObject(tenant.Id, resourceId, StorageObjectLifecycleStates.DeleteRequested);
        missingKey.ObjectKey = null;
        var active = CreateStorageObject(tenant.Id, resourceId, StorageObjectLifecycleStates.Active);
        var otherResource = CreateStorageObject(tenant.Id, Guid.CreateVersion7(), StorageObjectLifecycleStates.DeleteRequested);
        var alreadyDeleted = CreateStorageObject(tenant.Id, resourceId, StorageObjectLifecycleStates.DeleteRequested);
        alreadyDeleted.MarkDeleted(null, new DateTime(2026, 6, 23, 10, 0, 0, DateTimeKind.Utc));
        context.StorageObjects.AddRange(matching, missingKey, active, otherResource, alreadyDeleted);
        await context.SaveChangesAsync();
        var repository = new StorageObjectRepository(context);

        var results = await repository.ListDeleteRequestedForResourceAsync(
            tenant.Id,
            ResourceKinds.StorageObject,
            resourceId,
            limit: 10,
            CancellationToken.None);

        await Assert.That(results.Select(storageObject => storageObject.Id)).IsEquivalentTo([matching.Id, missingKey.Id]);
    }

    private static StorageObject CreateStorageObject(Guid tenantId, Guid resourceId, string lifecycleState) => new()
    {
        Id = Guid.CreateVersion7(),
        TenantId = tenantId,
        Tenant = null!,
        FileTypeId = (int)FileTypeEnum.Image,
        FileType = null!,
        Provider = StorageProviders.Local,
        ObjectKey = $"tenants/{tenantId:N}/{Guid.CreateVersion7():N}.png",
        Uri = "/images/redacted.png",
        FullName = "redacted.png",
        SafeDisplayName = "redacted.png",
        Extension = ".png",
        Size = 100,
        Visibility = StorageObjectVisibilities.PublicImage,
        Purpose = StorageObjectPurposes.LegacyImage,
        LifecycleState = lifecycleState,
        OwningResourceKind = ResourceKinds.StorageObject,
        OwningResourceId = resourceId,
        CreatedAt = new DateTime(2026, 6, 23, 9, 0, 0, DateTimeKind.Utc)
    };
}
