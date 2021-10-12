namespace SkyActions.Twitch {

    public interface ITwitchBotHandler {
        public void OnChannelPointRedemption(string username, string redemptionName);
        public void OnBits(string username, int bitCount, string bitType);
        public void OnChat(string username, string message);
        public void OnSubscribe(string username, bool resubscriber = false, bool gifted = false, string? giftGiver = null);
        public void OnFollow(string username);
        public void OnRaid(string username, int count);
    }
}