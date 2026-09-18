namespace BreganTwitchBot.Domain.DTOs.Api
{
    public class GetPublicCommandResponse
    {
        public required string CommandName { get; set; }
        public required string[] Aliases { get; set; }
    }
}
