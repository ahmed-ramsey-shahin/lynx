using Lynx.IdentityService.Api.Requests;
using Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUserPassword;
using Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser;
using Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser;
using Lynx.IdentityService.Application.Features.Identity.Queries.GetUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lynx.IdentityService.Api.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class UsersController(ISender sender) : ApiController
    {
        [HttpPost("users")]
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

        [HttpGet("users/me")]
        [Authorize]
        public async Task<IActionResult> GetUser(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetUserQuery(), cancellationToken);
            return result.Match(Ok, Problem);
        }

        [HttpPut("users/me/password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(
            [FromBody] ChangeUserPasswordRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new ChangeUserPasswordCommand()
            {
                UserId = request.UserId,
                NewPassword = request.NewPassword,
                OldPassword = request.OldPassword
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPut("users/me/username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername(
            [FromBody] ChangeUsernameRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new ChangeUsernameCommand()
            {
                UserId = request.UserId,
                Password = request.Password,
                NewUsername = request.NewUsername
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPost("users/me/deletions")]
        public async Task<IActionResult> DeleteUser(
            [FromBody] DeleteUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new DeleteUserCommand()
            {
                Password = request.Password,
                UserId = request.UserId,
                HasConfirmed = request.HasConfirmed
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPost("activations")]
        public async Task<IActionResult> ActivateAccount(
            [FromBody] ActivateUserRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new ActivateUserCommand()
            {
                ActivationCode = request.ActivationCode
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }
    }
}
