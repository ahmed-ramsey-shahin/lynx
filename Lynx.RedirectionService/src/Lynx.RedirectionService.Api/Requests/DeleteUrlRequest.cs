namespace Lynx.RedirectionService.Api.Requests
{
    public sealed record DeleteUrlRequest
    {
        public Guid UrlId { get; init; }
        public Guid UserId { get; init; }
    }
}
