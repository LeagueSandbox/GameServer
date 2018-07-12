namespace LeagueSandbox.GameServer.Logic.GameObjects.Other
{
    public class Announce
    {
        public bool IsAnnounced { get; private set; }
        public long EventTime { get; private set; }
        private Announces _messageId;
        private bool _isMapSpecific;

        public Announce(long eventTime, Announces id, bool isMapSpecific)
        {
            IsAnnounced = false;
            EventTime = eventTime;
            _messageId = id;
            _isMapSpecific = isMapSpecific;
        }

        public void Execute()
        {
            Game.PacketNotifier.NotifyAnnounceEvent(_messageId, _isMapSpecific);
            IsAnnounced = true;
        }
    }

    public enum Announces : byte
    {
        WELCOME_TO_SR = 0x77,
        THIRY_SECONDS_TO_MINIONS_SPAWN = 0x78,
        MINIONS_HAVE_SPAWNED = 0x7F,
        MINIONS_HAVE_SPAWNED2 = 0x76
    }
}
