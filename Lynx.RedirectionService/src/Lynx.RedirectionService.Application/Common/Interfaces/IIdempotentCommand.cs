namespace Lynx.RedirectionService.Application.Common.Interfaces
{
    public interface IIdempotentCommand
    {
        string IdempotencyKey { get; }
    }
}
