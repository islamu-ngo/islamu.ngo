// ABOUTME: Shared E2E helpers for extracting command response outcomes without coupling to scenario seeds.
// ABOUTME: Moved here from EventApiScenario during Phase 10 Event product teardown.

using Explore.Application.Responses;

namespace Explore.Blazor.Client.E2ETests.Helpers;

internal static class ApiResponseHelpers
{
    public static Guid SuccessfulId(BaseCommandResponseOfGuid response, string operation)
    {
        EnsureSuccess(response, operation);
        return response.Id is { } id && id != Guid.Empty
            ? id
            : throw new InvalidOperationException($"API returned no id while {operation}.");
    }

    public static void EnsureSuccess(BaseCommandResponseOfGuid response, string operation)
    {
        if (response.Success != true)
        {
            throw new InvalidOperationException($"API failed while {operation}: {response.Message}");
        }
    }
}
