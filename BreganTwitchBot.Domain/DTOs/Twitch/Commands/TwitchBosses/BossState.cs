namespace BreganTwitchBot.Domain.DTOs.Twitch.Commands.TwitchBosses
{
    public class BossState
    {
        public required List<(string Username, string UserId)> ViewersJoined { get; set; } = [];
        public required List<string> TwitchMods { get; set; } = [];
        public required bool BossCountdownEnabled { get; set; }
        public required bool BossInProgress { get; set; }
    }
}
