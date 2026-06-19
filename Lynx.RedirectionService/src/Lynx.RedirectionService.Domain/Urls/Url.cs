using Lynx.RedirectionService.Domain.Common;
using Lynx.RedirectionService.Domain.Common.Results;

namespace Lynx.RedirectionService.Domain.Urls
{
    public sealed class Url : EventfulEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string LongUrl { get; private set; } = null!;
        public string Alias { get; private set; } = null!;
        public DateTimeOffset ExpirationDate { get; private set; }

        private Url()
        {}

        private Url(Guid id, Guid userId, string longUrl, string alias, DateTimeOffset expirationDate)
        {
            Id = id;
            UserId = userId;
            LongUrl = longUrl;
            Alias = alias;
            ExpirationDate = expirationDate;
        }

        public static Result<Url> Create(Guid id, Guid userId, string longUrl, string alias, DateTimeOffset expirationDate, TimeProvider timeProvider)
        {
            if (Guid.Empty == id)
            {
                return UrlErrors.IdRequired;
            }

            if (Guid.Empty == userId)
            {
                return UrlErrors.UserIdRequired;
            }

            if (string.IsNullOrWhiteSpace(longUrl))
            {
                return UrlErrors.UrlRequired;
            }

            if (string.IsNullOrWhiteSpace(alias))
            {
                return UrlErrors.AliasRequired;
            }

            if (expirationDate < timeProvider.GetUtcNow())
            {
                return UrlErrors.ExpirationDateInvalid;
            }

            return new Url(id, userId, longUrl, alias, expirationDate);
        }

        public Result<Deleted> Delete()
        {
            if (IsDeleted)
            {
                return Result.Deleted;
            }

            IsDeleted = true;
            return Result.Deleted;
        }
    }
}
