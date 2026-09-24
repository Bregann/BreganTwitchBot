using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Services;
using BreganTwitchBot.DomainTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

namespace BreganTwitchBot.DomainTests.Api
{
    [TestFixture]
    public class AuthServiceTests
    {
        private PostgreSqlContainer _postgresContainer;
        private AppDbContext _dbContext;
        private Mock<IEnvironmentalSettingHelper> _environmentalSettingHelper;
        private Mock<IHttpClientFactory> _httpClientFactory;

        private AuthService _authService;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            Environment.SetEnvironmentVariable("JwtKey", "a-test-signing-key-that-is-long-enough-for-hmac-sha256");
            Environment.SetEnvironmentVariable("JwtValidIssuer", "test-issuer");
            Environment.SetEnvironmentVariable("JwtValidAudience", "test-audience");

            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .WithCleanUp(true)
                .Build();

            await _postgresContainer.StartAsync();
        }

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseLazyLoadingProxies()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await DatabaseSeedHelper.SeedDatabase(_dbContext);

            _environmentalSettingHelper = new Mock<IEnvironmentalSettingHelper>();
            _httpClientFactory = new Mock<IHttpClientFactory>();

            _environmentalSettingHelper.Setup(x => x.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPIClientID)).Returns("test-client-id");
            _environmentalSettingHelper.Setup(x => x.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPISecret)).Returns("test-secret");
            _environmentalSettingHelper.Setup(x => x.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.WebsiteTwitchOAuthRedirectUri)).Returns("https://bot.bregan.me/api/Auth/Callback");

            _authService = new AuthService(_dbContext, _environmentalSettingHelper.Object, _httpClientFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
        }

        private async Task<User> CreateUser(string twitchUserId = "123", string twitchUsername = "someviewer")
        {
            var user = new User
            {
                TwitchUserId = twitchUserId,
                TwitchUsername = twitchUsername,
                FirstLoggedInAt = DateTime.UtcNow,
                LastLoggedInAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        private async Task<string> CreateRefreshToken(string userId, bool revoked = false, int expiresInDays = 7)
        {
            var token = Guid.NewGuid().ToString();

            _dbContext.UserRefreshTokens.Add(new UserRefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
                IsRevoked = revoked
            });

            await _dbContext.SaveChangesAsync();
            return token;
        }

        [Test]
        public void BuildTwitchLoginUrl_IncludesTheStateAndClientId()
        {
            var url = _authService.BuildTwitchLoginUrl("some-state-value");

            Assert.Multiple(() =>
            {
                Assert.That(url, Does.StartWith("https://id.twitch.tv/oauth2/authorize"));
                Assert.That(url, Does.Contain("client_id=test-client-id"));
                Assert.That(url, Does.Contain("state=some-state-value"));
                Assert.That(url, Does.Contain("response_type=code"));
            });
        }

        [Test]
        public void BuildTwitchLoginUrl_WithoutConfiguration_Throws()
        {
            _environmentalSettingHelper.Setup(x => x.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPIClientID)).Returns((string?)null);

            Assert.Throws<InvalidOperationException>(() => _authService.BuildTwitchLoginUrl("state"));
        }

        [Test]
        public async Task RefreshToken_ValidToken_IssuesNewTokens()
        {
            var user = await CreateUser();
            var token = await CreateRefreshToken(user.Id);

            var result = await _authService.RefreshToken(token);

            Assert.Multiple(() =>
            {
                Assert.That(result.AccessToken, Is.Not.Empty);
                Assert.That(result.RefreshToken, Is.Not.EqualTo(token));
            });
        }

        [Test]
        public async Task RefreshToken_RotatesTheOldTokenOut()
        {
            var user = await CreateUser();
            var token = await CreateRefreshToken(user.Id);

            await _authService.RefreshToken(token);

            var oldToken = await _dbContext.UserRefreshTokens.FirstAsync(x => x.Token == token);
            Assert.That(oldToken.IsRevoked, Is.True);
        }

        [Test]
        public async Task RefreshToken_ReusingARevokedToken_RevokesEverySession()
        {
            // a revoked token coming back usually means it was stolen, so every
            // session for that user is dropped rather than just refusing this one
            var user = await CreateUser();
            var firstToken = await CreateRefreshToken(user.Id);
            var secondToken = await CreateRefreshToken(user.Id);

            await _authService.RefreshToken(firstToken);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.RefreshToken(firstToken));

            var stillLive = await _dbContext.UserRefreshTokens.CountAsync(x => x.UserId == user.Id && !x.IsRevoked);
            Assert.That(stillLive, Is.EqualTo(0), "every token for the user should be revoked");
            Assert.That(secondToken, Is.Not.Empty);
        }

        [Test]
        public async Task RefreshToken_ExpiredToken_IsRejected()
        {
            var user = await CreateUser();
            var token = await CreateRefreshToken(user.Id, expiresInDays: -1);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _authService.RefreshToken(token));
        }

        [Test]
        public void RefreshToken_UnknownToken_IsRejected()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _authService.RefreshToken("not-a-real-token"));
        }

        [Test]
        public async Task Logout_RevokesTheToken()
        {
            var user = await CreateUser();
            var token = await CreateRefreshToken(user.Id);

            await _authService.LogoutAsync(token);

            var stored = await _dbContext.UserRefreshTokens.FirstAsync(x => x.Token == token);
            Assert.That(stored.IsRevoked, Is.True);
        }

        [Test]
        public async Task Logout_UnknownToken_DoesNotThrow()
        {
            Assert.DoesNotThrowAsync(async () => await _authService.LogoutAsync("not-a-real-token"));
            await Task.CompletedTask;
        }

        [Test]
        public async Task GetCurrentUser_ReturnsTheUser()
        {
            await CreateUser("999", "coolviewer");

            var result = await _authService.GetCurrentUserAsync("999");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.TwitchUsername, Is.EqualTo("coolviewer"));
                Assert.That(result.BroadcasterOfChannels, Is.Empty);
            });
        }

        [Test]
        public async Task GetCurrentUser_ForABroadcaster_ListsTheirChannel()
        {
            await CreateUser(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            var result = await _authService.GetCurrentUserAsync(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId);

            Assert.That(result!.BroadcasterOfChannels, Does.Contain(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName));
        }

        [Test]
        public async Task GetCurrentUser_UnknownUser_ReturnsNull()
        {
            var result = await _authService.GetCurrentUserAsync("nobody");

            Assert.That(result, Is.Null);
        }
    }
}
