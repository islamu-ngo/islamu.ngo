ABOUTME: End-to-end CQRS patterns showing command, query, handler, validator, and specification.
ABOUTME: Use as structural reference — adapt naming to actual feature context.

# Complete CQRS Examples

## Command Pattern (Create)

```csharp
// 1. Command (Application/Features/{Feature}/Commands/Create/)
public record CreateEventCommand(
    string Title,
    string? Description,
    DateTime StartDate) : IRequest<BaseCommandResponse<Guid>>, IAuthorizedRequest
{
    public Guid UserId { get; set; }
}

// 2. Validator (same folder, manually instantiated in handler)
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StartDate).GreaterThan(DateTime.UtcNow);
    }
}

// 3. Handler
public class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IMapper mapper) : IRequestHandler<CreateEventCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        CreateEventCommand request, CancellationToken ct)
    {
        var validator = new CreateEventCommandValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = mapper.Map<Event>(request);
        entity.CreatedBy = request.UserId;
        await eventRepository.AddAsync(entity, ct);
        return BaseCommandResponse<Guid>.Success(entity.Id);
    }
}
```

## Query Pattern (List with Specification)

```csharp
var specification = BlogQuerySpecification.Empty
    .ForTenant(request.TenantId)
    .PublishedAt(DateTime.UtcNow)
    .SortByPublishedAtDescending();

int totalCount = await blogRepository.CountAsync(specification, cancellationToken);
IReadOnlyList<Blog> blogs = await blogRepository.ListAsync(
    specification,
    (page - 1) * pageSize,
    pageSize,
    cancellationToken);
```

## Key Patterns

| Pattern | Rule |
|---------|------|
| Validator | Manually instantiated — never injected via DI |
| Repository | Returns entities — handler maps to DTOs |
| Command response | `BaseCommandResponse<Guid>` for creates |
| Cache | `HybridCache.GetOrCreateAsync` in query handlers |
| Specification | Immutable builder — `With*()` returns new instance |
| Auth | `IAuthorizedRequest` interface, `UserId` set by pipeline behavior |

## Related

- [handler-patterns.md](handler-patterns.md)
- [validation-integration.md](validation-integration.md)
- [query-patterns.md](query-patterns.md)
