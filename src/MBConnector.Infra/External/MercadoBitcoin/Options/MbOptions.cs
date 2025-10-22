namespace MBConnector.Infra.External.MercadoBitcoin.Options;

public sealed class MbOptions
{
    public string BaseUrl { get; set; } = "";
    public AuthOptions Auth { get; set; } = new();

    public sealed class AuthOptions
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public int RenewSkewSeconds { get; set; } = 60;
    }
}