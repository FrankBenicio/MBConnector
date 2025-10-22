using Flurl;
using Flurl.Http;
using MBConnector.Data.Dtos;
using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Options;

namespace MBConnector.Infra.External.MercadoBitcoin
{
    public sealed class MercadoBitcoinApiService : IMercadoBitcoinApiService
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IOptions<MbOptions> _options;

        public MercadoBitcoinApiService(ITokenProvider tokenProvider, IOptions<MbOptions> options)
        {
            _tokenProvider = tokenProvider;
            _options = options;
        }

        public async Task<IEnumerable<AccountResponse>> GetAccountsAsync()
        {
            var baseUrl = _options.Value.BaseUrl.TrimEnd('/');

            var accessToken = await _tokenProvider.GetAccessTokenAsync();

            var accounts = await baseUrl
                   .AppendPathSegment("accounts")
                   .WithOAuthBearerToken(accessToken)
                   .WithTimeout(TimeSpan.FromSeconds(30))
                   .GetJsonAsync<IEnumerable<AccountResponse>>();

            return accounts;
        }

        public async Task<IEnumerable<PositionResponse>> GetPositionsAsync(
            string accountId,
            string? symbols = null)
        {
            var baseUrl = _options.Value.BaseUrl.TrimEnd('/');
            var url = baseUrl
                .AppendPathSegments("accounts", accountId, "positions");

            if (!string.IsNullOrWhiteSpace(symbols))
                url = url.SetQueryParam("symbols", symbols);

            var accessToken = await _tokenProvider.GetAccessTokenAsync();

            var response = await url
                .WithOAuthBearerToken(accessToken)
                .WithTimeout(TimeSpan.FromSeconds(30))
                .GetJsonAsync<IEnumerable<PositionResponse>>();

            return response;
        }
    }
}