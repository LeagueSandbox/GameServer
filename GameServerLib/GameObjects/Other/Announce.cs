using GameServerCore.Domain;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Announce: IAnnounce
    {
        public bool IsAnnounced { get; private set; }
        public long EventTime { get; }
        private Announces _messageId;
        private bool _isMapSpecific;
        private Game _game;

        public Announce(Game game, long eventTime, Announces id, bool isMapSpecific)
        {
            IsAnnounced = false;
            EventTime = eventTime;
            _messageId = id;
            _isMapSpecific = isMapSpecific;
            _game = game;
        }

        public void Execute()
        {
            _game.PacketNotifier.NotifyAnnounceEvent(_game.Map.Id, _messageId, _isMapSpecific);
            IsAnnounced = true;
        }
    }
}
