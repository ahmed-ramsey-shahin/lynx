using Lynx.RedirectionService.Domain.Common;
using Lynx.RedirectionService.Domain.Common.Results;

namespace Lynx.RedirectionService.Domain.Urls
{
    public sealed class Url : EventfulEntity
    {
        public Guid Id { get; private set; }
        public string LongUrl { get; private set; } = null!;
        public string Alias { get; private set; } = null!;
        public DateTimeOffset ExpirationDate { get; private set; }

        private Url()
        {}

        private Url(Guid id, string longUrl, string alias, DateTimeOffset expirationDate)
        {
            Id = id;
            LongUrl = longUrl;
            Alias = alias;
            ExpirationDate = expirationDate;
        }

        public static Result<Url> Create(Guid id, string longUrl, string alias, DateTimeOffset expirationDate)
        {
            return new Url(id, longUrl, alias, expirationDate);
        }
    }
}
