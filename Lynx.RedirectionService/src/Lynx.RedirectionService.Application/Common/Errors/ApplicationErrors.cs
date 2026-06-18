using Lynx.RedirectionService.Domain.Common.Results;

namespace Lynx.RedirectionService.Application.Common.Errors
{
    public static class ApplicationErrors
    {
        public static Error AliasAlreadyExists => Error.Validation("Url.AiasAlreadyExists", "This alias already exists. Please choose a different one.");
    }
}
