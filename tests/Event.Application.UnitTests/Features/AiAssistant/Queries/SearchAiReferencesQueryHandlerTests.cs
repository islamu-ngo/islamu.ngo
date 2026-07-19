// ABOUTME: Unit tests for actor-backed AI reference search normalization and DTO mapping.
// ABOUTME: Verifies bounded actor repository calls and lightweight organization/actor output.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Features.AiAssistant.Handlers.Queries;
using Explore.Application.Features.AiAssistant.Requests.Queries;
using Explore.Domain;
using Explore.Domain.Enums;
using NSubstitute;

namespace Event.Application.UnitTests.Features.AiAssistant.Queries;

public sealed class SearchAiReferencesQueryHandlerTests
{
    private readonly IActorRepository _actorRepository = Substitute.For<IActorRepository>();
    private readonly SearchAiReferencesQueryHandler _handler;

    public SearchAiReferencesQueryHandlerTests()
    {
        _actorRepository.SearchAiReferenceActorsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _handler = new SearchAiReferencesQueryHandler(_actorRepository);
    }

    [Test]
    public async Task Handle_WhenSearchTermIsTooShort_ReturnsEmptyWithoutRepositoryCall()
    {
        var result = await _handler.Handle(new SearchAiReferencesQuery { SearchTerm = " a ", Limit = 10 }, CancellationToken.None);

        await Assert.That(result).IsEmpty();
        await _actorRepository.DidNotReceiveWithAnyArgs().SearchAiReferenceActorsAsync(default!, default, default);
    }

    [Test]
    public async Task Handle_ClampsLimitAndMapsLightweightActorFields()
    {
        var actorId = Guid.NewGuid();
        var actors = new List<Actor>
        {
            new()
            {
                Id = actorId,
                ActorTypeId = (int)ActorTypeEnum.User,
                ActorType = new ActorType { Id = (int)ActorTypeEnum.User, MasterCode = "USER", FullName = "User" },
                TenantId = Guid.NewGuid(),
                Tenant = null!,
                UserId = Guid.NewGuid(),
                Pii = new ActorPii { DisplayName = "Amina Speaker", Handle = "amina.example" },
            }
        };
        _actorRepository.SearchAiReferenceActorsAsync("amina", SearchAiReferencesQueryHandler.MaxLimit, Arg.Any<CancellationToken>())
            .Returns(actors);

        var result = await _handler.Handle(new SearchAiReferencesQuery { SearchTerm = "  amina  ", Limit = 999 }, CancellationToken.None);

        await _actorRepository.Received(1).SearchAiReferenceActorsAsync(
            "amina",
            SearchAiReferencesQueryHandler.MaxLimit,
            Arg.Any<CancellationToken>());
        await Assert.That(result.Count).IsEqualTo(1);
        var reference = result.Single();
        await Assert.That(reference.Kind).IsEqualTo("Actor");
        await Assert.That(reference.ReferenceId).IsEqualTo(actorId);
        await Assert.That(reference.DisplayName).IsEqualTo("Amina Speaker");
        await Assert.That(reference.Summary).IsEqualTo("@amina.example");
    }

    [Test]
    public async Task Handle_MapsActorAndOrganizationReferencesWithLightweightFields()
    {
        var actorId = Guid.NewGuid();
        var organizationActorId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var actors = new List<Actor>
        {
            new()
            {
                Id = organizationActorId,
                ActorTypeId = (int)ActorTypeEnum.Organization,
                ActorType = new ActorType { Id = (int)ActorTypeEnum.Organization, MasterCode = "ORGANIZATION", FullName = "Organization" },
                TenantId = Guid.NewGuid(),
                Tenant = null!,
                OrganizationId = organizationId,
                Pii = new ActorPii { DisplayName = "Community Center" },
                Description = "A short organization summary."
            },
            new()
            {
                Id = actorId,
                ActorTypeId = (int)ActorTypeEnum.User,
                ActorType = new ActorType { Id = (int)ActorTypeEnum.User, MasterCode = "USER", FullName = "User" },
                TenantId = Guid.NewGuid(),
                Tenant = null!,
                UserId = Guid.NewGuid(),
                Pii = new ActorPii { DisplayName = "Amina Speaker", Handle = "amina.example" },
            }
        };
        _actorRepository.SearchAiReferenceActorsAsync("amina", 10, Arg.Any<CancellationToken>())
            .Returns(actors);

        var result = await _handler.Handle(new SearchAiReferencesQuery { SearchTerm = "amina", Limit = 10 }, CancellationToken.None);

        await Assert.That(result.Count).IsEqualTo(2);
        var organization = result.Single(reference => reference.Kind == "Organization");
        await Assert.That(organization.ReferenceId).IsEqualTo(organizationId);
        await Assert.That(organization.DisplayName).IsEqualTo("Community Center");
        await Assert.That(organization.Summary).IsEqualTo("A short organization summary.");
        var actor = result.Single(reference => reference.Kind == "Actor");
        await Assert.That(actor.ReferenceId).IsEqualTo(actorId);
        await Assert.That(actor.DisplayName).IsEqualTo("Amina Speaker");
        await Assert.That(actor.Summary).IsEqualTo("@amina.example");
    }

    [Test]
    public async Task Handle_WhenLimitIsNotPositive_UsesDefaultLimit()
    {
        _actorRepository.SearchAiReferenceActorsAsync("lecture", SearchAiReferencesQueryHandler.DefaultLimit, Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(new SearchAiReferencesQuery { SearchTerm = "lecture", Limit = 0 }, CancellationToken.None);

        await _actorRepository.Received(1).SearchAiReferenceActorsAsync(
            "lecture",
            SearchAiReferencesQueryHandler.DefaultLimit,
            Arg.Any<CancellationToken>());
    }
}
