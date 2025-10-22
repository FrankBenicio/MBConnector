using Flurl;
using Flurl.Http;
using MBConnector.Data.Dtos;
using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Options;

namespace MBConnector.Infra.External.MercadoBitcoin
{
    public sealed class MercadoBitcoinAuthService : IMercadoBitcoinAuthService
    {
        private readonly IOptions<MbOptions> _opts;

        public MercadoBitcoinAuthService(IOptions<MbOptions> opts) => _opts = opts;

        public async Task<AuthResponse> AuthorizeAsync()
        {
            var baseUrl = _opts.Value.BaseUrl.TrimEnd('/');
            var body = new
            {
                login = _opts.Value.Auth.Login,
                password = _opts.Value.Auth.Password
            };

            var response = await baseUrl
                .AppendPathSegment("authorize")
                .PostJsonAsync(body)
                .ReceiveJson<AuthResponse>();

            return response;
        }
    }
}