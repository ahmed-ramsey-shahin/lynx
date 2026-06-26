using Lynx.RedirectionService.Api.Requests;
using Lynx.RedirectionService.Api.Services;
using Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl;
using Lynx.RedirectionService.Application.Features.Urls.Commands.DeleteUrl;
using Lynx.RedirectionService.Application.Features.Urls.Queries.GetUrlByAlias;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lynx.RedirectionService.Api.Controllers
{
    [ApiController]
    [Route("/api/urls")]
    public class UrlsController(ISender sender) : ApiController
    {
        [HttpGet("/{alias}")]
        public async Task<IActionResult> RedirectToLongUrl(string alias)
        {
            var query = new GetUrlByAliasQuery()
            {
                Alias = alias
            };
            var result = await sender.Send(query);
            return result.Match(dto => Redirect(dto.LongUrl), Problem);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUrl(
            [FromServices] IUserService userService,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            [FromBody] CreateUrlRequest request
        )
        {
            var command = new CreateUrlCommand()
            {
                LongUrl = request.LongUrl,
                UserId = userService.UserId!.Value,
                CustomAlias = request.CustomAlias,
                ExpirationInDays = request.ExpirationInDays,
                IdempotencyKey = idempotencyKey
            };
            var result = await sender.Send(command);
            return result.Match(
                _ => StatusCode(StatusCodes.Status201Created),
                Problem
            );
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteUrl(
            [FromServices] IUserService userService,
            Guid id
        )
        {
            var command = new DeleteUrlCommand()
            {
                UserId = userService.UserId!.Value,
                UrlId = id
            };
            var result = await sender.Send(command);
            return result.Match(_ => NoContent(), Problem);
        }
    }
}
