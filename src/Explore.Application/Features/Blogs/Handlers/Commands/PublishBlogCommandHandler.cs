// ABOUTME: Handles Blog publication by loading the tenant-owned aggregate and setting publication UTC.
// ABOUTME: Persists the aggregate through IBlogRepository without side effects outside Application.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Requests.Commands;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Domain;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Commands;

public sealed class PublishBlogCommandHandler(IBlogRepository blogRepository)
    : IRequestHandler<PublishBlogCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        PublishBlogCommand request,
        CancellationToken cancellationToken)
    {
        var validator = new PublishBlogCommandValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var blog = await blogRepository.GetByTenantAndIdAsync(request.TenantId, request.BlogId, cancellationToken);
        if (blog is null)
        {
            throw new NotFoundException(nameof(Blog), request.BlogId);
        }

        blog.Publish(DateTime.UtcNow);
        var updated = await blogRepository.UpdateAsync(blog, cancellationToken);

        return new BaseCommandResponse<Guid>
        {
            Id = updated.Id,
            Success = true,
            Message = "Blog published successfully."
        };
    }
}
