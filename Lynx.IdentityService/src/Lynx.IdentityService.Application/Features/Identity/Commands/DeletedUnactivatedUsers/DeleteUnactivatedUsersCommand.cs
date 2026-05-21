using MediatR;

namespace Lynx.IdentityService.Application.Features.Identity.Commands.DeletedUnactivatedUsers
{
    public sealed record DeleteUnactivatedUsersCommand : IRequest;
}
