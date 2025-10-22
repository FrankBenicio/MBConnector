using FluentAssertions;
using MBConnector.Data.Dtos;
using MBConnector.Data.Services;
using MBConnector.Infra.External.MercadoBitcoin;
using MBConnector.Infra.External.MercadoBitcoin.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace MBConnector.Tests
{
    public sealed class TokenProviderTests : IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly Mock<IMercadoBitcoinAuthService> _authMock;
        private readonly IOptions<MbOptions> _opts;

        public TokenProviderTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _authMock = new Mock<IMercadoBitcoinAuthService>(MockBehavior.Strict);

            _opts = Options.Create(new MbOptions
            {
                BaseUrl = "http://fake",
                Auth = new MbOptions.AuthOptions
                {
                    Login = "x",
                    Password = "y",
                    RenewSkewSeconds = 60
                }
            });
        }

        public void Dispose() => (_cache as IDisposable)?.Dispose();

        private static long EpochInSecondsFromNow(int seconds) =>
            DateTimeOffset.UtcNow.AddSeconds(seconds).ToUnixTimeSeconds();

        [Fact]
        public async Task GetAccessTokenAsync_Should_Return_From_Cache_When_Available()
        {
            // Arrange
            var cacheKey = "MB:AccessToken";
            _cache.Set(cacheKey, "cached-token", TimeSpan.FromMinutes(5));

            var provider = new TokenProvider(_cache, _authMock.Object, _opts);

            // Act
            var token = await provider.GetAccessTokenAsync();

            // Assert
            token.Should().Be("cached-token");
            _authMock.Verify(x => x.AuthorizeAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAccessTokenAsync_Should_Call_Authorize_When_Missing_And_Cache_It()
        {
            // Arrange
            _authMock
                .Setup(x => x.AuthorizeAsync())
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "fresh-token",
                    Expiration = EpochInSecondsFromNow(3600)
                });

            var provider = new TokenProvider(_cache, _authMock.Object, _opts);

            // Act
            var token1 = await provider.GetAccessTokenAsync();
            var token2 = await provider.GetAccessTokenAsync();

            // Assert
            token1.Should().Be("fresh-token");
            token2.Should().Be("fresh-token");
            _authMock.Verify(x => x.AuthorizeAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_Should_Be_ThreadSafe_Only_One_Authorize_Call()
        {
            // Arrange
            _authMock
                .Setup(x => x.AuthorizeAsync())
                .Returns(async () =>
                {
                    await Task.Delay(100);
                    return new AuthResponse
                    {
                        AccessToken = "fresh-token",
                        Expiration = EpochInSecondsFromNow(120)
                    };
                });

            var provider = new TokenProvider(_cache, _authMock.Object, _opts);

            // Act
            var tasks = Enumerable.Range(0, 20)
                .Select(_ => provider.GetAccessTokenAsync())
                .ToArray();

            await Task.WhenAll(tasks);

            // Assert
            tasks.Select(t => t.Result).Distinct().Should().ContainSingle().Which.Should().Be("fresh-token");
            _authMock.Verify(x => x.AuthorizeAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccessTokenAsync_Should_Apply_Minimum_TTL_When_Expiration_Too_Close()
        {
            // Arrange
            _authMock
                .Setup(x => x.AuthorizeAsync())
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "short-lived",
                    Expiration = EpochInSecondsFromNow(10)
                });

            var provider = new TokenProvider(_cache, _authMock.Object, _opts);

            // Act
            var token1 = await provider.GetAccessTokenAsync();
            token1.Should().Be("short-lived");
            _authMock.Verify(x => x.AuthorizeAsync(), Times.Once);

            await Task.Delay(1500);
            var token2 = await provider.GetAccessTokenAsync();

            // Assert
            token2.Should().Be("short-lived");
            _authMock.Verify(x => x.AuthorizeAsync(), Times.Once);
        }
    }
}