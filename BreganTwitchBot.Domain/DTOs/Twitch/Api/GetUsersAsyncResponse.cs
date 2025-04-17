namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetUsersAsyncResponse
    {
        public required List<User> Users { get; set; }
    }

    public class User
    {
        public required string Id { get; set; }
        public required string Login { get; set; }
        public required string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
