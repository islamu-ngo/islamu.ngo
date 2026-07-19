// ABOUTME: API-facing Blog DTOs for public published reads and editorial write payloads.
// ABOUTME: Keeps Blog contracts tenant-explicit without leaking Domain entities to handlers or clients.

namespace Explore.Application.DTOs.Blog;

public class BlogSummaryDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ThumbnailStorageObjectId { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public IReadOnlyList<Guid> CategoryIds { get; set; } = [];
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public sealed class BlogDetailDto : BlogSummaryDto
{
    public string ContentMarkdown { get; set; } = string.Empty;
    public Guid ConcurrencyStamp { get; set; }
}

public sealed class CreateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
    public Guid? ThumbnailStorageObjectId { get; set; }
    public IReadOnlyList<Guid> CategoryIds { get; set; } = [];
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
}

public sealed class UpdateBlogDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string ContentMarkdown { get; set; } = string.Empty;
    public Guid ExpectedConcurrencyStamp { get; set; }
    public Guid? ThumbnailStorageObjectId { get; set; }
    public IReadOnlyList<Guid> CategoryIds { get; set; } = [];
    public IReadOnlyList<Guid> TagIds { get; set; } = [];
}
