using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;

namespace BreganTwitchBot.Domain.Data.TwitchBot
{
    internal class TwitchBotConnection
    {
        public static TwitchClient Client = new();
        private static readonly ConnectionCredentials Credentials = new(AppConfig.BotName, AppConfig.BotOAuth);

        internal async Task Connect()
        {
            Log.Information("[Twitch Client] Attempting to connect to twitch chat");
            Client = new TwitchClient();
            Client.Initialize(Credentials, AppConfig.BroadcasterName);
            Client.Connect();

            Client.OnConnected += ClientOnConnected;
            Client.OnDisconnected += ClientOnDisconnected;
            Client.OnReconnected += ClientOnReconnected; ;
            Client.OnError += ClientOnError;

            await Task.Delay(-1);
        }

        private void ClientOnReconnected(object? sender, OnReconnectedEventArgs e)
        {
            Log.Information("[Twitch Client] Connected to Twitch Chat");
        }

        private void ClientOnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            Client.JoinChannel(AppConfig.BroadcasterName);
            Client.SendMessage(AppConfig.BroadcasterName, "hello currys (Succesfully cOnnected!)");
            Log.Information("[Twitch Client] Connected to Twitch Chat");
        }

        private void ClientOnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            Log.Warning($"[Twitch Client] Bot disconnected from channel. Reason: {e}");
        }

        private void ClientOnError(object? sender, OnErrorEventArgs e)
        {
            Log.Warning($"[Twitch Client] Bot error - {e.Exception}");
        }
    }

    internal class TwitchPubSubConnection
    {
        public static TwitchPubSub PubSubClient = new();

        internal void Connect()
        {
            PubSubClient = new TwitchPubSub();
            PubSubClient.OnPubSubServiceConnected += PubSubClientPubSubServiceConnected;
            PubSubClient.OnListenResponse += PubSubClientListenResponse;
            PubSubClient.OnPubSubServiceClosed += PubSubClientPubSubServiceClosed;
            PubSubClient.ListenToBitsEventsV2(AppConfig.TwitchChannelID);
            PubSubClient.ListenToFollows(AppConfig.TwitchChannelID);
            PubSubClient.ListenToSubscriptions(AppConfig.TwitchChannelID);
            PubSubClient.ListenToChannelPoints(AppConfig.TwitchChannelID);
            PubSubClient.ListenToPredictions(AppConfig.TwitchChannelID);

            PubSubClient.Connect();
        }

        private async void PubSubClientPubSubServiceClosed(object? sender, EventArgs e)
        {
            Log.Warning("[Twitch PubSub] PubSub Service Closed");

            try
            {
                PubSubClient.ListenToBitsEventsV2(AppConfig.TwitchChannelID);
                PubSubClient.ListenToFollows(AppConfig.TwitchChannelID);
                PubSubClient.ListenToSubscriptions(AppConfig.TwitchChannelID);
                PubSubClient.ListenToChannelPoints(AppConfig.TwitchChannelID);
                PubSubClient.ListenToPredictions(AppConfig.TwitchChannelID);
                PubSubClient.Connect();
                Log.Warning("[Twitch PubSub] PubSub Service Reconnected");
            }
            catch (Exception pubSubEx) //if its not recoverable, force the bot to quit
            {
                await DiscordHelper.SendMessage(AppConfig.DiscordEventChannelID, "pubsub?");
                Log.Fatal($"[Twitch PubSub] Error Reconnecting - {pubSubEx}");
                return;
            }
        }

        private void PubSubClientListenResponse(object? sender, TwitchLib.PubSub.Events.OnListenResponseArgs e)
        {
            if (e.Successful)
            {
                Log.Information($"[Twitch PubSub] Successfully verified listening to topic: {e.Topic}");
            }
            else
            {
                Log.Fatal($"[Twitch PubSub] Failed to listen! Error: {e.Response.Error}");
            }
        }

        private void PubSubClientPubSubServiceConnected(object? sender, EventArgs e)
        {
            Log.Information("[Twitch PubSub] Connected");
            PubSubClient.SendTopics(AppConfig.BroadcasterOAuth);
        }
    }

    internal class TwitchApiConnection
    {
        public static TwitchAPI ApiClient = new();

        internal void Connect()
        {
            try
            {
                ApiClient = new TwitchAPI();
                ApiClient.Settings.ClientId = AppConfig.TwitchAPIClientID;
                ApiClient.Settings.AccessToken = AppConfig.BroadcasterOAuth;
            }
            catch (BadGatewayException) //These are the two main errors that occur when the Twitch API goes down
            {
                Log.Fatal("[Twitch API Connection] BadGatewayException Error connecting to the Twitch API");
            }
            catch (InternalServerErrorException)
            {
                Log.Fatal("[Twitch API Connection] InternalServerErrorException Error connecting to the Twitch API");
            }
        }
    }
}