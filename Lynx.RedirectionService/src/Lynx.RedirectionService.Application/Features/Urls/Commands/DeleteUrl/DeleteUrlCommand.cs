using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;

namespace Lynx.RedirectionService.Application.Features.Urls.Commands.DeleteUrl
{
    public sealed record DeleteUrlCommand : IRequest<Result<Deleted>>
    {
        public Guid UrlId { get; init; }
        public Guid UserId { get; init; }
    }
}
