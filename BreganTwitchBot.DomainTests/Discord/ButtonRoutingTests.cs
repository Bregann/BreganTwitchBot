using BreganTwitchBot.Domain.DTOs.Discord.Events;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Services.Discord;
using Moq;

namespace BreganTwitchBot.DomainTests.Discord
{
    /// <summary>
    /// Every button press arrives through one handler, so a bad merge there silently breaks
    /// every feature behind it. These only check the routing, not what each feature does.
    /// </summary>
    [TestFixture]
    public class ButtonRoutingTests
    {
        private Mock<IDiscordGiveawayData> _giveawayData;
        private Mock<IDiscordSelfAssignRoleData> _selfAssignRoleData;

        private DiscordEventHelperService _eventHelperService;

        [SetUp]
        public void Setup()
        {
            _giveawayData = new Mock<IDiscordGiveawayData>();
            _selfAssignRoleData = new Mock<IDiscordSelfAssignRoleData>();

            _eventHelperService = new DiscordEventHelperService(
                null!,
                new Mock<IConfigHelperService>().Object,
                new Mock<IDiscordHelperService>().Object,
                new Mock<IDiscordRoleManagerService>().Object,
                new Mock<IDiscordUserLookupService>().Object,
                _selfAssignRoleData.Object,
                _giveawayData.Object,
                new Mock<IDiscordMessageModerationService>().Object,
                new Mock<IDiscordCustomCommandService>().Object);
        }

        private Task<(string MessageToSend, bool Ephemeral)> Press(string customId)
        {
            return _eventHelperService.HandleButtonPressEvent(new ButtonPressedEvent
            {
                GuildId = 1,
                UserId = 2,
                Username = "someone",
                CustomId = customId
            }, null!);
        }

        [TestCase("enter")]
        [TestCase("check")]
        [TestCase("draw")]
        public async Task GiveawayButtons_ReachTheGiveaway(string action)
        {
            _giveawayData.Setup(x => x.EnterGiveaway(1, 2, "abc")).ReturnsAsync(("entered", true));
            _giveawayData.Setup(x => x.CheckEntries(1, 2, "abc")).ReturnsAsync(("checked", true));
            _giveawayData.Setup(x => x.DrawWinner(1, 2, "abc")).ReturnsAsync(("drawn", false));

            var (message, _) = await Press($"giveaway-abc-{action}");

            Assert.That(message, Is.Not.EqualTo("invalid button"));
        }

        [Test]
        public async Task SelfRoleButton_TogglesTheRole()
        {
            _selfAssignRoleData.Setup(x => x.ToggleRole(1, 2, 7)).ReturnsAsync(("role added", true));

            var (message, _) = await Press("selfrole-7");

            Assert.That(message, Is.EqualTo("role added"));
        }

        [Test]
        public async Task SelfRoleButton_WithABadId_IsInvalid()
        {
            var (message, _) = await Press("selfrole-notanumber");

            Assert.That(message, Is.EqualTo("invalid button"));
            _selfAssignRoleData.Verify(x => x.ToggleRole(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<int>()), Times.Never());
        }

        [Test]
        public async Task UnknownButton_IsInvalid()
        {
            var (message, _) = await Press("nothing-we-know");

            Assert.That(message, Is.EqualTo("invalid button"));
        }
    }
}
