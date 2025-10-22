using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MBConnector.Infra.External.MercadoBitcoin
{
    public sealed class TokenProvider : ITokenProvider
    {
        private const string CacheKey = "MB:AccessToken";
        private readonly IMemoryCache _cache;
        private readonly IMercadoBitcoinAuthService _auth;
        private readonly IOptions<MbOptions> _opts;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public TokenProvider(IMemoryCache cache, IMercadoBitcoinAuthService auth, IOptions<MbOptions> opts)
        {
            _cache = cache;
            _auth = auth;
            _opts = opts;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_cache.TryGetValue(CacheKey, out string token))
                return token;

            await _lock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(CacheKey, out token))
                    return token;

                var authResult = await _auth.AuthorizeAsync();

                var expiration = DateTimeOffset.FromUnixTimeSeconds(authResult.Expiration);

                var ttl = expiration - DateTimeOffset.UtcNow
                          - TimeSpan.FromSeconds(_opts.Value.Auth.RenewSkewSeconds);
                if (ttl < TimeSpan.FromSeconds(30))
                    ttl = TimeSpan.FromSeconds(30);

                _cache.Set(CacheKey, authResult.AccessToken, ttl);
                return authResult.AccessToken;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}