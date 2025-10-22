using MBConnector.Data.Dtos;

namespace MBConnector.Data.Services
{
    public interface IMercadoBitcoinAuthService
    {
        Task<AuthResponse> AuthorizeAsync();
    }
}