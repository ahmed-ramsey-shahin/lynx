namespace Lynx.RedirectionService.Application.Features.Urls.Commands.CreateUrl
{
    public sealed record CreateUrlCommand
    {
        public string LongUrl { get; init; } = null!;
        public string? CustomAlias { get; init; }
        public int? ExpirationInDays { get; init; }
    }
}
