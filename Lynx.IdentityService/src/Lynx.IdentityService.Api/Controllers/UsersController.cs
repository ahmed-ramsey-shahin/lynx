using Lynx.IdentityService.Api.Requests;
using Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser;
using Lynx.IdentityService.Application.Features.Identity.Queries.GetUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lynx.IdentityService.Api.Controllers
{
    [ApiController]
    [Route("/api/auth/users")]
    public class UsersController(ISender sender) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new CreateUserCommand()
            {
                Email = request.Email,
                IdempotencyKey = idempotencyKey,
                Username = request.Username,
                Password = request.Password
            }, cancellationToken);
            return result.Match(
                id => Created(string.Empty, id),
                Problem
            );
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetUser(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetUserQuery(), cancellationToken);
            return result.Match(Ok, Problem);
        }
    }
}
