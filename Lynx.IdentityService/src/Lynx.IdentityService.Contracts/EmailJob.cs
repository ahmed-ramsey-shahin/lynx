namespace Lynx.IdentityService.Contracts
{
    public sealed record EmailJob
    (
        string To,
        string Username,
        string Subject,
        string Body
    );
}
