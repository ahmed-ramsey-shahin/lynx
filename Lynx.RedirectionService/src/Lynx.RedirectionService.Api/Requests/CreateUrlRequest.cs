namespace Lynx.RedirectionService.Api.Requests
{
    public sealed record CreateUrlRequest
    {
        public string LongUrl { get; init; } = null!;
        public string? CustomAlias { get; init; }
        public int? ExpirationInDays { get; init; }
        public string IdempotencyKey { get; init; } = null!;
    }
}
