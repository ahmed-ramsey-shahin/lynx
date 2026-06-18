namespace Lynx.RedirectionService.Application.Features.Urls.Dtos
{
    public sealed record UrlDto
    {
        public string Alias { get; init; } = null!;
        public string LongUrl { get; init; } = null!;
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
