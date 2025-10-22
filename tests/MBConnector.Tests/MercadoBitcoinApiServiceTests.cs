using FluentAssertions;
using MBConnector.Data.Dtos;
using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MBConnector.Tests
{
    public sealed class MercadoBitcoinApiServiceTests : IDisposable
    {
        private readonly WireMockServer _server;
        private readonly Mock<ITokenProvider> _tokenProviderMock;

        public MercadoBitcoinApiServiceTests()
        {
            _server = WireMockServer.Start();
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }

        private static IOptions<MbOptions> OptionsFor(string baseUrl) =>
            Options.Create(new MbOptions
            {
                BaseUrl = baseUrl,
                Auth = new MbOptions.AuthOptions
                {
                    Login = "x",
                    Password = "y",
                    RenewSkewSeconds = 60
                }
            });

        [Fact]
        public async Task GetAccountsAsync_Should_Return_Accounts_And_Send_Bearer_Header()
        {
            // Arrange
            const string token = "token-123";
            _tokenProviderMock.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(token);

            var accountsPayload = new[]
            {
            new AccountResponse
            {
                Currency = "BRL",
                CurrencySign = "R$",
                Id = "a322205ace882ef800553118e5000066",
                Name = "Mercado Bitcoin",
                Type = "live"
            }
        };

            _server
                .Given(Request.Create()
                    .WithPath("/accounts")
                    .UsingGet()
                    .WithHeader("Authorization", $"Bearer {token}"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(accountsPayload)));

            var svc = new MercadoBitcoinApiService(_tokenProviderMock.Object, OptionsFor(_server.Url));

            // Act
            var result = await svc.GetAccountsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var acc = result.First();
            acc.Id.Should().Be("a322205ace882ef800553118e5000066");
            acc.Currency.Should().Be("BRL");
            acc.Type.Should().Be("live");

            _tokenProviderMock.Verify(x => x.GetAccessTokenAsync(), Times.Once);

            _server.LogEntries.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPositionsAsync_Should_Return_Positions_With_Symbols_Query_And_Bearer_Header()
        {
            // Arrange
            const string token = "token-xyz";
            const string accountId = "a322205ace882ef800553118e5000066";
            const string symbols = "BTC-BRL";

            _tokenProviderMock.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(token);

            var positionsPayload = new[]
            {
            new PositionResponse
            {
                AvgPrice = 380m,
                Category = "limit",
                Id = "27",
                Instrument = "BTC-BRL",
                Quantity = 0.001m,
                Side = "buy"
            }
        };

            _server
                .Given(Request.Create()
                    .WithPath($"/accounts/{accountId}/positions")
                    .WithParam("symbols", symbols)
                    .UsingGet()
                    .WithHeader("Authorization", $"Bearer {token}"))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(positionsPayload)));

            var svc = new MercadoBitcoinApiService(_tokenProviderMock.Object, OptionsFor(_server.Url));

            // Act
            var result = await svc.GetPositionsAsync(accountId, symbols);

            // Assert
            result.Should().NotBeNull().And.HaveCount(1);
            var p = result.First();
            p.Id.Should().Be("27");
            p.Instrument.Should().Be("BTC-BRL");
            p.Quantity.Should().Be(0.001m);
            p.Category.Should().Be("limit");
            p.Side.Should().Be("buy");

            _tokenProviderMock.Verify(x => x.GetAccessTokenAsync(), Times.Once);
            _server.LogEntries.Should().HaveCount(1);
        }
    }
}