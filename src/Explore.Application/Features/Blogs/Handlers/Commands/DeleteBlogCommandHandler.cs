// ABOUTME: Handles Blog soft deletion by loading the tenant-owned aggregate and marking it deleted.
// ABOUTME: Persists deletion state through IBlogRepository so repository filters remain the source of visibility.

using Explore.Application.Contracts.Persistence;
using Explore.Application.Exceptions;
using Explore.Application.Features.Blogs.Requests.Commands;
using Explore.Application.Features.Blogs.Validators;
using Explore.Application.Responses;
using Explore.Domain;
using FluentValidation;
using MediatR;

namespace Explore.Application.Features.Blogs.Handlers.Commands;

public sealed class DeleteBlogCommandHandler(IBlogRepository blogRepository)
    : IRequestHandler<DeleteBlogCommand, BaseCommandResponse<Guid>>
{
    public async Task<BaseCommandResponse<Guid>> Handle(
        DeleteBlogCommand request,
        CancellationToken cancellationToken)
    {
        var validator = new DeleteBlogCommandValidator();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var blog = await blogRepository.GetByTenantAndIdAsync(request.TenantId, request.BlogId, cancellationToken);
        if (blog is null)
        {
            throw new NotFoundException(nameof(Blog), request.BlogId);
        }

        blog.MarkDeleted(null, DateTime.UtcNow);
        var updated = await blogRepository.UpdateAsync(blog, cancellationToken);

        return new BaseCommandResponse<Guid>
        {
            Id = updated.Id,
            Success = true,
            Message = "Blog deleted successfully."
        };
    }
}
