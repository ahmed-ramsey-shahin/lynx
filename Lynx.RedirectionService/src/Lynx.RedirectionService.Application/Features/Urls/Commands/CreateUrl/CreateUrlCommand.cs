using Lynx.RedirectionService.Application.Common.Interfaces;
using Lynx.RedirectionService.Domain.Common.Results;
using MediatR;

namespace Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl
{
    public sealed record CreateUrlCommand : IIdempotentCommand, IRequest<Result<Guid>>
    {
        public Guid UserId { get; init; }
        public string LongUrl { get; init; } = null!;
        public string? CustomAlias { get; init; }
        public int? ExpirationInDays { get; init; }
        public string IdempotencyKey { get; init; } = null!;
    }
}
