using MBConnector.Data.Dtos;

namespace MBConnector.Data.Services
{
    public interface IMercadoBitcoinApiService
    {
        Task<IEnumerable<AccountResponse>> GetAccountsAsync();

        Task<IEnumerable<PositionResponse>> GetPositionsAsync(
            string accountId,
            string? symbols = null);
    }
}