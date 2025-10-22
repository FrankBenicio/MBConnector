namespace MBConnector.Data.Services
{
    public interface ITokenProvider
    {
        Task<string> GetAccessTokenAsync();
    }
}