using FluentAssertions;
using MBConnector.Data.Dtos;
using MBConnector.Infra.External.MercadoBitcoin;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static MBConnector.Infra.External.MercadoBitcoin.Options.MbOptions;

namespace MBConnector.Tests
{
    public sealed class MercadoBitcoinAuthServiceTests : IDisposable
    {
        private readonly WireMockServer _server;

        public MercadoBitcoinAuthServiceTests()
        {
            _server = WireMockServer.Start();
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }

        [Fact]
        public async Task AuthorizeAsync_Should_Return_Valid_Token()
        {
            // Arrange
            var expected = new AuthResponse
            {
                AccessToken = "01GF442ATTVP4M6M0XGHQYT544",
                Expiration = 1732224000
            };

            _server
                .Given(Request.Create()
                    .WithPath("/authorize")
                    .UsingPost()
                    .WithBody(JsonSerializer.Serialize(new
                    {
                        login = "test-login",
                        password = "test-pass"
                    })))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(JsonSerializer.Serialize(expected)));

            var options = Options.Create(new MbOptions
            {
                BaseUrl = _server.Url,
                Auth = new AuthOptions
                {
                    Login = "test-login",
                    Password = "test-pass"
                }
            });

            var service = new MercadoBitcoinAuthService(options);

            // Act
            var result = await service.AuthorizeAsync();

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be(expected.AccessToken);
            result.Expiration.Should().Be(expected.Expiration);

            _server.LogEntries.Count().Should().Be(1);
        }
    }
}