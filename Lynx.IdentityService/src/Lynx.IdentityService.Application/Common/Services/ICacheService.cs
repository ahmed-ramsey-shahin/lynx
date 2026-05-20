namespace Lynx.IdentityService.Application.Common.Services
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiration=null, CancellationToken cancellationToken=default);
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken=default);
        Task RemoveAsync(string key, CancellationToken cancellationToken=default);
        Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expiration=null, CancellationToken cancellationToken=default);
    }
}
