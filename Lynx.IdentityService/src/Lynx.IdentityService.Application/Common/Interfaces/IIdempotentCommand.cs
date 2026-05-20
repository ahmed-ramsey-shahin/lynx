namespace Lynx.IdentityService.Application.Common.Interfaces
{
    public interface IIdempotentCommand
    {
        string IdempotencyKey { get; }
    }
}
