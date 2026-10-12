// ABOUTME: Caps API integration-test concurrency to keep resource-heavy test hosts stable.
// ABOUTME: Applies one TUnit parallel limiter across the entire integration-test assembly.

using TUnit.Core;
using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<Event.Api.IntegrationTests.ApiIntegrationParallelLimit>]

namespace Event.Api.IntegrationTests;

internal sealed class ApiIntegrationParallelLimit : IParallelLimit
{
    public int Limit => 4;
}
