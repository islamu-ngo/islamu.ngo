// ABOUTME: Lightweight AI reference search result used for bounded actor and organization discovery.
// ABOUTME: Exposes safe display metadata only; full actor content stays out of AI reference results.

namespace Explore.Application.DTOs.Ai;

public sealed record AiReferenceSearchResultDto(
    string Kind,
    Guid ReferenceId,
    string DisplayName,
    string? Summary,
    DateOnly? FirstSessionDate,
    DateOnly? LastSessionDate,
    string? Visibility,
    string? Format);
