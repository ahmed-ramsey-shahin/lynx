using System.ComponentModel.DataAnnotations;
using Lynx.IdentityService.Api.Requests;
using Lynx.IdentityService.Application.Common.Services;
using Lynx.IdentityService.Application.Features.Identity.Commands.ActivateUser;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUsername;
using Lynx.IdentityService.Application.Features.Identity.Commands.ChangeUserPassword;
using Lynx.IdentityService.Application.Features.Identity.Commands.CreateUser;
using Lynx.IdentityService.Application.Features.Identity.Commands.DeleteUser;
using Lynx.IdentityService.Application.Features.Identity.Commands.GenerateToken;
using Lynx.IdentityService.Application.Features.Identity.Commands.PasswordReset;
using Lynx.IdentityService.Application.Features.Identity.Commands.RefreshToken;
using Lynx.IdentityService.Application.Features.Identity.Commands.RequestPasswordReset;
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
            [FromHeader(Name = "Idempotency-Key")] [Required] string idempotencyKey,
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
        public async Task<IActionResult> GetUser(
            [FromServices] IUserService userService,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new GetUserQuery()
            {
                UserId = userService.UserId!.Value.ToString()
            }, cancellationToken);
            return result.Match(Ok, Problem);
        }

        [HttpPut("users/me/password")]
        [Authorize]
        public async Task<IActionResult> ChangeUserPassword(
            [FromBody] ChangeUserPasswordRequest request,
            [FromServices] IUserService userService,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new ChangeUserPasswordCommand()
            {
                UserId = userService.UserId!.Value,
                NewPassword = request.NewPassword,
                OldPassword = request.OldPassword
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPut("users/me/username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername(
            [FromBody] ChangeUsernameRequest request,
            [FromServices] IUserService userService,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new ChangeUsernameCommand()
            {
                UserId = userService.UserId!.Value,
                Password = request.Password,
                NewUsername = request.NewUsername
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPost("users/me/deletions")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(
            [FromBody] DeleteUserRequest request,
            [FromServices] IUserService userService,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new DeleteUserCommand()
            {
                Password = request.Password,
                UserId = userService.UserId!.Value,
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

        [HttpPost("password-reset-requests")]
        public async Task<IActionResult> RequestPasswordReset(
            [FromHeader(Name = "Idempotency-Key")] [Required] string idempotencyKey,
            [FromBody] RequestPasswordResetRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new RequestPasswordResetCommand()
            {
                Email = request.Email,
                IdempotencyKey = idempotencyKey
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPost("password-resets")]
        public async Task<IActionResult> PasswordReset(
            [FromBody] PasswordResetRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new PasswordResetCommand()
            {
                Email = request.Email,
                NewPassword = request.NewPassword,
                Code = request.Code
            }, cancellationToken);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPost("tokens")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new GenerateTokenCommand()
            {
                Username = request.Username,
                Password = request.Password
            }, cancellationToken);
            return result.Match(Ok, Problem);
        }

        [HttpPut("tokens")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken
        )
        {
            var result = await sender.Send(new RefreshTokenCommand()
            {
                ExpiredAccessToken = request.ExpiredAccessToken,
                RefreshToken = request.RefreshToken
            }, cancellationToken);
            return result.Match(Ok, Problem);
        }
    }
}
