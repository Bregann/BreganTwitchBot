﻿using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Events
{
    public interface ITwitchEventHandlerService
    {
        Task HandleChannelCheerEvent(BitsCheeredParams cheerParams);
        Task HandleChannelResubscribeEvent(ChannelResubscribeParams resubscribeParams);
    }
}
