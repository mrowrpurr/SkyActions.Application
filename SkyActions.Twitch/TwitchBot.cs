using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace SkyActions.Twitch {

    public class TwitchBot {
        // Provided handler for events.
        // Right now you only provide one handler instance.
        ITwitchBotHandler TwitchBotHandler { get; set; }

        // Configuration for connecting to Twitch
        TwitchConfig? Config;

        // Twitch Clients
        TwitchClient? Client;
        TwitchPubSub? PubSubClient;

        public TwitchBot(ITwitchBotHandler handler) {
            TwitchBotHandler = handler;
            Config = ReadConfig();
            if (Config != null) {
                // Setup Twitch WebSocket and Pub/Sub listeners
                Client = SetupTwitchClient(Config!);
                PubSubClient = new TwitchPubSub();
                RegisterTwitchEvents(Client, PubSubClient);
            }
        }

        public void Connect() {
            if (Client != null) Client.Connect();
            if (PubSubClient != null) PubSubClient.Connect();
        }

        public void Disconnect() {
            if (Client != null) Client.Disconnect();
            if (PubSubClient != null) PubSubClient.Disconnect();
        }

        TwitchConfig? ReadConfig() {
            using var reader = new StreamReader("config.json");
            return JsonConvert.DeserializeObject<TwitchConfig>(reader.ReadToEnd());
        }

        TwitchClient SetupTwitchClient(TwitchConfig config) {
            ConnectionCredentials credentials = new ConnectionCredentials(config.Username, config.AccessToken);
            var clientOptions = new ClientOptions { MessagesAllowedInPeriod = 750, ThrottlingPeriod = TimeSpan.FromSeconds(30) };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            var twitchClient = new TwitchClient(customClient);
            twitchClient.Initialize(credentials, config.ChannelName);
            return twitchClient;
        }

        void RegisterTwitchEvents(TwitchClient client, TwitchPubSub pubsub) {
            pubsub.OnPubSubServiceConnected += PubSubClient_OnPubSubServiceConnected;
            // client.OnMessageReceived += Client_OnMessageReceived;
            // client.OnNewSubscriber += Client_OnNewSubscriber;
            // client.OnReSubscriber += Client_OnReSubscriber;
            // client.OnGiftedSubscription += Client_OnGiftedSubscription;
            // client.OnRaidNotification += Client_OnRaidNotification;
            pubsub.OnRewardRedeemed += PubSubClient_OnRewardRedeemed;
            // pubsub.OnFollow += PubSubClient_OnFollow;
        }

        void PubSubClient_OnPubSubServiceConnected(object? sender, EventArgs e) {
            PubSubClient!.ListenToRewards(Config!.ChannelID);
            PubSubClient!.ListenToFollows(Config!.ChannelName);
            PubSubClient!.SendTopics(Config.AccessToken);
        }

        void PubSubClient_OnRewardRedeemed(object? sender, OnRewardRedeemedArgs e) {
            TwitchBotHandler.OnChannelPointRedemption(e.DisplayName, e.RewardTitle);
        }
    }
}
