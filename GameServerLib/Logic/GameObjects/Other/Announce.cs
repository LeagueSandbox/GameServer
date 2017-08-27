using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Announce
    {
        public bool IsAnnounced { get; private set; }
        public long EventTime { get; private set; }
        private readonly Announces _messageId;
        private readonly int _mapId;
        private readonly Game _game;

        public Announce(Game game, long eventTime, Announces id, int mapId)
        {
            IsAnnounced = false;
            EventTime = eventTime;
            _messageId = id;
            _mapId = mapId;
            _game = game;
        }

        public void Execute()
        {
            _game.PacketNotifier.NotifyAnnounceEvent(_messageId, _mapId);
            IsAnnounced = true;
        }
    }
}
