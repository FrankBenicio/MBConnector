using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MBConnector.Infra.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MbOptions>(configuration.GetSection("MercadoBitcoin"));

            services.AddTransient<ITokenProvider, TokenProvider>();
            services.AddTransient<IMercadoBitcoinAuthService, MercadoBitcoinAuthService>();
            services.AddTransient<IMercadoBitcoinApiService, MercadoBitcoinApiService>();

            return services;
        }
    }
}