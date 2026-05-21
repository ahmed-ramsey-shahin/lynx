using Lynx.IdentityService.Domain.Common.Results;
using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser
{
    public sealed record ActivateUserCommand : IRequest<Result<Updated>>
    {
        public string ActivationCode { get; init; } = null!;
    }
}
