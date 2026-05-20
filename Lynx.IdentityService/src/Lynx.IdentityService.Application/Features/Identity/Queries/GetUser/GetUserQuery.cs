using Lynx.IdentityService.Application.Common.Interfaces;
using Lynx.IdentityService.Application.Features.Identity.Dtos;
using Lynx.IdentityService.Domain.Common.Results;

namespace Lynx.IdentityService.Application.Features.Identity.Queries.GetUser
{
    public sealed record GetUserQuery : ICachedQuery<Result<UserDto>>
    {
        public string Username { get; init; } = null!;

        public string CacheKey => $"users:{Username}";

        public TimeSpan Expiration => TimeSpan.FromHours(1);
    }
}
