using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Announce
    {
        private bool _announced;
        private long _eventTime;
        private Announces _messageId;
        private bool _isMapSpecific;
        private Game _game;

        public Announce(Game game, long eventTime, Announces id, bool isMapSpecific)
        {
            _announced = false;
            _eventTime = eventTime;
            _messageId = id;
            _isMapSpecific = isMapSpecific;
            _game = game;
        }

        public bool IsAnnounced()
        {
            return _announced;
        }

        public long GetEventTime()
        {
            return _eventTime;
        }

        public void Execute()
        {
            _game.GetPacketNotifier().notifyAnnounceEvent(_messageId, _isMapSpecific);
            _announced = true;
        }
    }

    public enum Announces : byte
    {
        WelcomeToSR = 0x77,
        ThirySecondsToMinionsSpawn = 0x78,
        MinionsHaveSpawned = 0x7F,
        MinionsHaveSpawned2 = 0x76
    }
}
