using Lynx.RedirectionService.Domain.Common.Results;

namespace Lynx.RedirectionService.Domain.Urls
{
    public static class UrlErrors
    {
        public static Error IdRequired => Error.Validation("Url.IdRequired", "Url ID is required.");
        public static Error UrlRequired => Error.Validation("Url.UrlRequired", "The original url is required.");
        public static Error AliasRequired => Error.Validation("Url.AliasRequired", "The alias is required.");
    }
}
