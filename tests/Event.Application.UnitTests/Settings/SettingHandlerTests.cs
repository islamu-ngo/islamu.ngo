// ABOUTME: Unit tests for all generic settings command/query handlers (Phase A4).
// ABOUTME: Covers ResolveGroup, Update, BatchUpdate, Reset, Lock, Unlock handlers with mocked dependencies.

namespace Event.Application.UnitTests.Settings;

using System.Text.Json;
using Explore.Application.Contracts.Identity;
using Explore.Application.Contracts.Infrastructure;
using Explore.Application.Contracts.Persistence;
using Explore.Application.DTOs.Settings;
using Explore.Application.Exceptions;
using Explore.Application.Features.Settings.Handlers.Commands;
using Explore.Application.Features.Settings.Handlers.Queries;
using Explore.Application.Features.Settings.Requests.Commands;
using Explore.Application.Features.Settings.Requests.Queries;
using Explore.Application.Notifications;
using Explore.Application.Responses;
using Explore.Application.Settings;
using Explore.Domain;
using Explore.Domain.Constants;
using Explore.Domain.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

public class SettingHandlerTests
{
    private static readonly Guid TestTenantId = Guid.Parse("018e4e5c-7f00-7000-8000-000000000001");
    private static readonly Guid TestUserId = Guid.Parse("018e4e5c-7f00-7000-8000-000000000099");

    private static readonly string TestKey = GovernanceSettingKeys.Appearance.ThemeMode;
    private static readonly string TestIntKey = GovernanceSettingKeys.PublicExperiencePreferences.AnnouncementBarDismissedRevision;
    private static readonly string TestSecondaryKey = GovernanceSettingKeys.Appearance.Direction;

    private readonly IHierarchicalSettingsResolver _resolver;
    private readonly IUserPreferenceRepository _userPrefRepo;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAdminContext _adminContext;
    private readonly IMediator _mediator;

    public SettingHandlerTests()
    {
        _resolver = Substitute.For<IHierarchicalSettingsResolver>();
        _userPrefRepo = Substitute.For<IUserPreferenceRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _adminContext = Substitute.For<IAdminContext>();
        _mediator = Substitute.For<IMediator>();

        _tenantContext.TenantId.Returns(TestTenantId);
        _currentUserService.UserId.Returns(TestUserId);
        _currentUserService.IsAuthenticated.Returns(true);
    }

    // ──────────────────────────────────────────────
    // ResolveSettingGroupQueryHandler
    // ──────────────────────────────────────────────

    private ResolveSettingGroupQueryHandler CreateResolveHandler() =>
        new(_resolver, _tenantContext, _currentUserService, _adminContext,
            Substitute.For<ILogger<ResolveSettingGroupQueryHandler>>());

    [Test]
    public async Task ResolveGroup_UnknownCategory_ReturnsEmptySettings()
    {
        var handler = CreateResolveHandler();
        var query = new ResolveSettingGroupQuery { Category = "NonExistent", Scope = SettingScope.User };

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Category).IsEqualTo("NonExistent");
        await Assert.That(result.Settings).IsEmpty();
    }

    [Test]
    public async Task ResolveGroup_AppearanceCategory_ReturnsAllSettings()
    {
        SetupResolverBatchForAppearance();
        var handler = CreateResolveHandler();
        var query = new ResolveSettingGroupQuery { Category = "Appearance", Scope = SettingScope.User };

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Category).IsEqualTo("Appearance");
        await Assert.That(result.Settings.Count).IsEqualTo(7);
    }

    [Test]
    public async Task ResolveGroup_LockedSetting_CanEditIsFalse()
    {
        SetupResolverBatchWithLock();
        var handler = CreateResolveHandler();
        var query = new ResolveSettingGroupQuery { Category = "Appearance", Scope = SettingScope.User };

        var result = await handler.Handle(query, CancellationToken.None);

        var lockedSetting = result.Settings.FirstOrDefault(s => s.Key == TestKey);
        await Assert.That(lockedSetting).IsNotNull();
        await Assert.That(lockedSetting!.CanEdit).IsFalse();
        await Assert.That(lockedSetting.IsLocked).IsTrue();
    }

    [Test]
    public async Task ResolveGroup_UnauthorizedUser_CanEditIsFalse()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        SetupResolverBatchForAppearance();
        var handler = CreateResolveHandler();
        var query = new ResolveSettingGroupQuery { Category = "Appearance", Scope = SettingScope.User };

        var result = await handler.Handle(query, CancellationToken.None);

        var setting = result.Settings.Single(item => item.Key == TestKey);
        await Assert.That(setting.CanEdit).IsFalse();
        await Assert.That(setting.Reason).IsEqualTo("Insufficient permissions");
    }

    // ──────────────────────────────────────────────
    // UpdateSettingCommandHandler
    // ──────────────────────────────────────────────

    private UpdateSettingCommandHandler CreateUpdateHandler(ICerbosConfigResolver? cerbosConfigResolver = null) =>
        new(_resolver, _userPrefRepo, _tenantContext, _currentUserService,
            _adminContext, _mediator, Substitute.For<ILogger<UpdateSettingCommandHandler>>(), cerbosConfigResolver);

    [Test]
    public async Task Update_UnknownKey_Fails()
    {
        var handler = CreateUpdateHandler();
        var cmd = new UpdateSettingCommand { Key = "nonexistent.key", Value = "x", Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not found");
    }

    [Test]
    public async Task Update_ScopeOutOfRange_Fails()
    {
        var handler = CreateUpdateHandler();
        var cmd = new UpdateSettingCommand
        {
            Key = GovernanceSettingKeys.Email.SmtpSecurity,
            Value = "None",
            Scope = SettingScope.User
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("cannot be overridden at User scope");
    }

    [Test]
    public async Task Update_Unauthorized_Fails()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        var handler = CreateUpdateHandler();
        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("Authentication required");
    }

    [Test]
    public async Task Update_TenantScope_RequiresAdmin()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(false);
        _adminContext.IsInstanceAdminAsync(Arg.Any<CancellationToken>()).Returns(false);
        var handler = CreateUpdateHandler();
        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("administrators");
    }

    [Test]
    public async Task Update_InvalidStringValue_Fails()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestKey, false);

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "invalid-mode", Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not allowed");
    }

    [Test]
    public async Task Update_InvalidIntegerValue_Fails()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestIntKey, false);

        var cmd = new UpdateSettingCommand { Key = TestIntKey, Value = "abc", Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not a valid integer");
    }

    [Test]
    public async Task Update_LockedSetting_Fails()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestKey, true, SettingSource.TenantLocked);

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("Locked by");
    }

    [Test]
    public async Task Update_UserScope_WritesViaRepository()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestKey, false);
        _userPrefRepo.GetByUserAndKey(TestTenantId, TestUserId, TestKey)
            .Returns((UserPreference?)null);

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _userPrefRepo.Received(1).Create(Arg.Is<UserPreference>(p =>
            p.SettingKey == TestKey && p.UserId == TestUserId));
    }

    [Test]
    public async Task Update_UserScope_UpdatesExisting()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestKey, false);
        var existing = new UserPreference
        {
            Id = Guid.NewGuid(),
            TenantId = TestTenantId,
            Tenant = null!,
            UserId = TestUserId,
            SettingKey = TestKey,
            Value = "\"light\"",
            CreatedAt = DateTime.UtcNow
        };
        _userPrefRepo.GetByUserAndKey(TestTenantId, TestUserId, TestKey).Returns(existing);

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _userPrefRepo.Received(1).Update(Arg.Is<UserPreference>(p => p.Id == existing.Id));
    }

    [Test]
    public async Task Update_TenantScope_WritesViaResolver()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        SetupResolverMetadata(TestKey, false);
        var handler = CreateUpdateHandler();

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.Tenant };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).SetValueAsync(
            TestKey, Arg.Any<string>(), SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_TenantCerbosEndpoint_InvalidatesTenantCerbosConfigCache()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var cerbosConfigResolver = Substitute.For<ICerbosConfigResolver>();
        SetupResolverMetadata(GovernanceSettingKeys.Cerbos.CustomEndpoint, false);
        var handler = CreateUpdateHandler(cerbosConfigResolver);

        var cmd = new UpdateSettingCommand
        {
            Key = GovernanceSettingKeys.Cerbos.CustomEndpoint,
            Value = "https://tenant-cerbos.example.com:443",
            Scope = SettingScope.Tenant
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        cerbosConfigResolver.Received(1).InvalidateCache(TestTenantId);
    }

    [Test]
    public async Task Update_TenantCerbosEndpoint_NormalizesBareHostBeforePersisting()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        SetupResolverMetadata(GovernanceSettingKeys.Cerbos.CustomEndpoint, false);
        var handler = CreateUpdateHandler();

        var cmd = new UpdateSettingCommand
        {
            Key = GovernanceSettingKeys.Cerbos.CustomEndpoint,
            Value = "tenant-cerbos.example.com:443",
            Scope = SettingScope.Tenant
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).SetValueAsync(
            Arg.Is<string>(GovernanceSettingKeys.Cerbos.CustomEndpoint),
            Arg.Is<string>(value =>
                JsonSerializer.Deserialize<string>(value) == "https://tenant-cerbos.example.com:443"),
            Arg.Is<SettingScope>(SettingScope.Tenant),
            Arg.Is<Guid>(TestTenantId),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_PublicExperiencePrimaryOrganization_WritesTenantOverrideViaResolver()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var key = GovernanceSettingKeys.PublicExperience.PrimaryOrganizationId;
        var organizationId = Guid.NewGuid().ToString();
        SetupResolverMetadata(key, false);
        var handler = CreateUpdateHandler();

        var cmd = new UpdateSettingCommand { Key = key, Value = organizationId, Scope = SettingScope.Tenant };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).SetValueAsync(
            key, Arg.Any<string>(), SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_PublicExperienceHomeBlocks_WritesVersionedJsonViaResolver()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var key = GovernanceSettingKeys.PublicExperience.HomeBlocks;
        const string presetsJson = "{\"schemaVersion\":1,\"blocks\":[{\"id\":\"featured\",\"label\":\"Featured\"}]}";
        SetupResolverMetadata(key, false);
        var handler = CreateUpdateHandler();

        var cmd = new UpdateSettingCommand { Key = key, Value = presetsJson, Scope = SettingScope.Tenant };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).SetValueAsync(
            key, presetsJson, SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Update_PublishesNotification()
    {
        var handler = CreateUpdateHandler();
        SetupResolverMetadata(TestKey, false);

        var cmd = new UpdateSettingCommand { Key = TestKey, Value = "dark", Scope = SettingScope.User };
        await handler.Handle(cmd, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Any<Explore.Application.Notifications.SettingChangedNotification>(),
            Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // UpdateSettingBatchCommandHandler
    // ──────────────────────────────────────────────

    private UpdateSettingBatchCommandHandler CreateBatchHandler(ICerbosConfigResolver? cerbosConfigResolver = null) =>
        new(_resolver, _userPrefRepo, _tenantContext, _currentUserService,
            _adminContext, _mediator, Substitute.For<ILogger<UpdateSettingBatchCommandHandler>>(), cerbosConfigResolver);

    [Test]
    public async Task Batch_EmptyValues_ReturnsSuccess()
    {
        var handler = CreateBatchHandler();
        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Appearance",
            Values = new Dictionary<string, string>(),
            Scope = SettingScope.User
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Results).IsEmpty();
    }

    [Test]
    public async Task Batch_Unauthorized_AllFail()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        var handler = CreateBatchHandler();
        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Appearance",
            Values = new Dictionary<string, string> { { TestKey, "dark" } },
            Scope = SettingScope.User
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Results.All(r => !r.Applied)).IsTrue();
    }

    [Test]
    public async Task Batch_BestEffort_SkipsLockedAppliesRest()
    {
        var handler = CreateBatchHandler();
        SetupResolverBatchForBatchCommand(lockedKey: TestKey);

        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Appearance",
            Values = new Dictionary<string, string>
            {
                { TestKey, "dark" },
                { TestSecondaryKey, "rtl" }
            },
            Scope = SettingScope.User,
            Mode = BatchUpdateMode.BestEffort
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        var lockedResult = result.Results.First(r => r.Key == TestKey);
        var appliedResult = result.Results.First(r => r.Key == TestSecondaryKey);
        await Assert.That(lockedResult.Applied).IsFalse();
        await Assert.That(lockedResult.SkipReason).IsNotNull();
        await Assert.That(appliedResult.Applied).IsTrue();
    }

    [Test]
    public async Task Batch_Strict_RejectsAllIfAnyBlocked()
    {
        var handler = CreateBatchHandler();
        SetupResolverBatchForBatchCommand(lockedKey: TestKey);

        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Appearance",
            Values = new Dictionary<string, string>
            {
                { TestKey, "dark" },
                { TestSecondaryKey, "rtl" }
            },
            Scope = SettingScope.User,
            Mode = BatchUpdateMode.Strict
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Results.All(r => !r.Applied)).IsTrue();
    }

    [Test]
    public async Task Batch_KeyNotInCategory_Skipped()
    {
        var handler = CreateBatchHandler();
        SetupResolverBatchForBatchCommand();

        // Use a key from another category
        var foreignKey = GovernanceSettingKeys.Email.SmtpSecurity;
        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Appearance",
            Values = new Dictionary<string, string> { { foreignKey, "some-value" } },
            Scope = SettingScope.User
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        var foreignResult = result.Results.First(r => r.Key == foreignKey);
        await Assert.That(foreignResult.Applied).IsFalse();
        await Assert.That(foreignResult.SkipReason).Contains("does not belong to category");
    }

    [Test]
    public async Task Batch_InstanceCerbosSettings_InvalidatesAllCerbosConfigCaches()
    {
        _adminContext.IsInstanceAdminAsync(Arg.Any<CancellationToken>()).Returns(true);
        var cerbosConfigResolver = Substitute.For<ICerbosConfigResolver>();
        var handler = CreateBatchHandler(cerbosConfigResolver);
        SetupResolverBatchForBatchCommand();

        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Cerbos",
            Values = new Dictionary<string, string>
            {
                { GovernanceSettingKeys.Cerbos.TenantCustomizationEnabled, "true" }
            },
            Scope = SettingScope.Instance
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        cerbosConfigResolver.Received(1).InvalidateCache();
    }

    [Test]
    public async Task Batch_CerbosEndpoints_NormalizesBareHostsBeforePersisting()
    {
        _adminContext.IsInstanceAdminAsync(Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateBatchHandler();
        SetupResolverBatchForBatchCommand();

        var cmd = new UpdateSettingBatchCommand
        {
            Category = "Cerbos",
            Values = new Dictionary<string, string>
            {
                { GovernanceSettingKeys.Cerbos.CustomEndpoint, "cerbosgrpc.example.com:443" },
                { GovernanceSettingKeys.Cerbos.CustomAdminEndpoint, "cerbosapi.example.com:3592" }
            },
            Scope = SettingScope.Instance
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).SetValueAsync(
            Arg.Is<string>(GovernanceSettingKeys.Cerbos.CustomEndpoint),
            Arg.Is<string>(value =>
                JsonSerializer.Deserialize<string>(value) == "https://cerbosgrpc.example.com:443"),
            Arg.Is<SettingScope>(SettingScope.Instance),
            Arg.Is<Guid>(Guid.Empty),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
        await _resolver.Received(1).SetValueAsync(
            Arg.Is<string>(GovernanceSettingKeys.Cerbos.CustomAdminEndpoint),
            Arg.Is<string>(value =>
                JsonSerializer.Deserialize<string>(value) == "https://cerbosapi.example.com:3592"),
            Arg.Is<SettingScope>(SettingScope.Instance),
            Arg.Is<Guid>(Guid.Empty),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // ResetSettingCommandHandler
    // ──────────────────────────────────────────────

    private ResetSettingCommandHandler CreateResetHandler() =>
        new(_resolver, _userPrefRepo, _tenantContext, _currentUserService,
            _adminContext, _mediator, Substitute.For<ILogger<ResetSettingCommandHandler>>());

    [Test]
    public async Task Reset_UnknownKey_Fails()
    {
        var handler = CreateResetHandler();
        var cmd = new ResetSettingCommand { Key = "nonexistent.key", Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not found");
    }

    [Test]
    public async Task Reset_InstanceScope_Fails()
    {
        var handler = CreateResetHandler();
        var cmd = new ResetSettingCommand { Key = TestKey, Scope = SettingScope.Instance };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("Cannot reset instance-level");
    }

    [Test]
    public async Task Reset_Unauthorized_Fails()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        var handler = CreateResetHandler();
        var cmd = new ResetSettingCommand { Key = TestKey, Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("Authentication required");
    }

    [Test]
    public async Task Reset_UserScope_CallsRepositoryRemove()
    {
        var handler = CreateResetHandler();
        SetupResolverMetadata(TestKey, false);
        _userPrefRepo.RemoveOverride(TestTenantId, TestUserId, TestKey).Returns(true);

        var cmd = new ResetSettingCommand { Key = TestKey, Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _userPrefRepo.Received(1).RemoveOverride(TestTenantId, TestUserId, TestKey);
    }

    [Test]
    public async Task Reset_UserScope_NoOverride_Fails()
    {
        var handler = CreateResetHandler();
        SetupResolverMetadata(TestKey, false);
        _userPrefRepo.RemoveOverride(TestTenantId, TestUserId, TestKey).Returns(false);

        var cmd = new ResetSettingCommand { Key = TestKey, Scope = SettingScope.User };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("No user override found");
    }

    [Test]
    public async Task Reset_TenantScope_CallsResolver()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        SetupResolverMetadata(TestKey, false);
        var handler = CreateResetHandler();

        var cmd = new ResetSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };
        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).RemoveOverrideAsync(
            TestKey, SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Reset_TenantScope_WhenSystemLocksDuringMutation_ReturnsStableFailure()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        SetupResolverMetadata(TestKey, false);
        _resolver.RemoveOverrideAsync(
                TestKey,
                SettingScope.Tenant,
                TestTenantId,
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new SettingSystemLockedException(TestKey));
        var handler = CreateResetHandler();

        var result = await handler.Handle(
            new ResetSettingCommand { Key = TestKey, Scope = SettingScope.Tenant },
            CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.FailureCode).IsEqualTo(SettingSystemLockedException.Code);
        await _mediator.DidNotReceive().Publish(
            Arg.Any<SettingChangedNotification>(),
            Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // LockSettingCommandHandler
    // ──────────────────────────────────────────────

    private LockSettingCommandHandler CreateLockHandler() =>
        new(_resolver, _tenantContext, _currentUserService, _adminContext,
            _mediator, Substitute.For<ILogger<LockSettingCommandHandler>>());

    [Test]
    public async Task Lock_UnknownKey_Fails()
    {
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = "nonexistent.key", Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not found");
    }

    [Test]
    public async Task Lock_NotLockable_Fails()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).LockAsync(
            TestKey, SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Lock_UserScope_Fails()
    {
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = TestKey, Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("only supported at Instance and Tenant");
    }

    [Test]
    public async Task Lock_Unauthorized_Fails()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(false);
        _adminContext.IsInstanceAdminAsync(Arg.Any<CancellationToken>()).Returns(false);
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("administrators");
    }

    [Test]
    public async Task Lock_Success_PublishesNotification()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        await handler.Handle(cmd, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<Explore.Application.Notifications.SettingChangedNotification>(n =>
                n.Key == TestKey),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Lock_InvalidatesCache()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateLockHandler();
        var cmd = new LockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        await handler.Handle(cmd, CancellationToken.None);

        _resolver.Received(1).InvalidateCache(SettingScope.Tenant, TestTenantId);
    }

    // ──────────────────────────────────────────────
    // UnlockSettingCommandHandler
    // ──────────────────────────────────────────────

    private UnlockSettingCommandHandler CreateUnlockHandler() =>
        new(_resolver, _tenantContext, _currentUserService, _adminContext,
            _mediator, Substitute.For<ILogger<UnlockSettingCommandHandler>>());

    [Test]
    public async Task Unlock_UnknownKey_Fails()
    {
        var handler = CreateUnlockHandler();
        var cmd = new UnlockSettingCommand { Key = "nonexistent.key", Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("not found");
    }

    [Test]
    public async Task Unlock_UserScope_Fails()
    {
        var handler = CreateUnlockHandler();
        var cmd = new UnlockSettingCommand { Key = TestKey, Scope = SettingScope.User };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("only supported at Instance and Tenant");
    }

    [Test]
    public async Task Unlock_Unauthorized_Fails()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(false);
        _adminContext.IsInstanceAdminAsync(Arg.Any<CancellationToken>()).Returns(false);
        var handler = CreateUnlockHandler();
        var cmd = new UnlockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Message).Contains("administrators");
    }

    [Test]
    public async Task Unlock_Success_CallsResolverAndInvalidatesCache()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateUnlockHandler();
        var cmd = new UnlockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        var result = await handler.Handle(cmd, CancellationToken.None);

        await Assert.That(result.Success).IsTrue();
        await _resolver.Received(1).UnlockAsync(
            TestKey, SettingScope.Tenant, TestTenantId,
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _resolver.Received(1).InvalidateCache(SettingScope.Tenant, TestTenantId);
    }

    [Test]
    public async Task Unlock_Success_PublishesNotification()
    {
        _adminContext.IsTenantAdminAsync(TestTenantId, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateUnlockHandler();
        var cmd = new UnlockSettingCommand { Key = TestKey, Scope = SettingScope.Tenant };

        await handler.Handle(cmd, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<Explore.Application.Notifications.SettingChangedNotification>(n =>
                n.Key == TestKey),
            Arg.Any<CancellationToken>());
    }

    // ──────────────────────────────────────────────
    // Helper setup methods
    // ──────────────────────────────────────────────

    private void SetupResolverMetadata(string key, bool isLocked,
        SettingSource source = SettingSource.SystemDefault)
    {
        var definition = SettingRegistry.Get(key)!;
        _resolver.ResolveWithMetadataAsync(key, Arg.Any<SettingContext>(), Arg.Any<CancellationToken>())
            .Returns(new ResolvedSetting
            {
                Key = key,
                Value = definition.DefaultValue,
                ValueType = definition.ValueType,
                Source = source,
                IsLocked = isLocked,
                Description = definition.Description,
                Category = definition.Category,
                AllowedValues = definition.AllowedValues is { Length: > 0 }
                    ? string.Join(",", definition.AllowedValues) : null
            });
    }

    private static string? JoinAllowedValues(string[]? values) =>
        values is { Length: > 0 } ? string.Join(",", values) : null;

    private void SetupResolverBatchForAppearance()
    {
        var definitions = SettingRegistry.GetByCategory("Appearance")!;
        var resolved = definitions.Select(d => new ResolvedSetting
        {
            Key = d.Key,
            Value = d.DefaultValue,
            ValueType = d.ValueType,
            Source = SettingSource.SystemDefault,
            IsLocked = false,
            Description = d.Description,
            Category = d.Category,
            AllowedValues = JoinAllowedValues(d.AllowedValues)
        }).ToList();

        _resolver.ResolveBatchAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<SettingContext>(), Arg.Any<CancellationToken>())
            .Returns(resolved);
    }

    private void SetupResolverBatchWithLock()
    {
        var definitions = SettingRegistry.GetByCategory("Appearance")!;
        var resolved = definitions.Select(d => new ResolvedSetting
        {
            Key = d.Key,
            Value = d.DefaultValue,
            ValueType = d.ValueType,
            Source = d.Key == TestKey ? SettingSource.TenantLocked : SettingSource.SystemDefault,
            IsLocked = d.Key == TestKey,
            Description = d.Description,
            Category = d.Category,
            AllowedValues = JoinAllowedValues(d.AllowedValues)
        }).ToList();

        _resolver.ResolveBatchAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<SettingContext>(), Arg.Any<CancellationToken>())
            .Returns(resolved);
    }

    private void SetupResolverBatchForBatchCommand(string? lockedKey = null)
    {
        _resolver.ResolveBatchAsync(
            Arg.Any<IEnumerable<string>>(), Arg.Any<SettingContext>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var keys = callInfo.Arg<IEnumerable<string>>().ToList();
                return keys.Select(k =>
                {
                    var def = SettingRegistry.Get(k);
                    return new ResolvedSetting
                    {
                        Key = k,
                        Value = def?.DefaultValue ?? "null",
                        ValueType = def?.ValueType ?? SettingValueType.String,
                        Source = k == lockedKey ? SettingSource.TenantLocked : SettingSource.SystemDefault,
                        IsLocked = k == lockedKey,
                        Description = def?.Description,
                        Category = def?.Category ?? "Unknown",
                        AllowedValues = JoinAllowedValues(def?.AllowedValues)
                    };
                }).ToList();
            });
    }
}
