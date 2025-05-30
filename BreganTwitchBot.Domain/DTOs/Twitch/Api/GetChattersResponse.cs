﻿namespace BreganTwitchBot.Domain.DTOs.Twitch.Api
{
    public class GetChattersResponse
    {
        public required List<Chatters> Chatters { get; set; }
    }

    public class Chatters
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
    }
}
