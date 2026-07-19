// ABOUTME: Resolves the appropriate AI provider strategy from registered strategies.
// ABOUTME: Follows the shared strategy-resolver convention used by provider integrations.

namespace Explore.Application.Contracts.Infrastructure.Ai;

public interface IAiProviderStrategyResolver
{
    IAiProviderStrategy? Resolve(int providerId);
}
