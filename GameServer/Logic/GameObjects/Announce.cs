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
        private bool Announced;
        private long EventTime;
        private Announces MessageId;
        private bool IsMapSpecific;

        public Announce(long eventTime, Announces id, bool isMapSpecific)
        {
            Announced = false;
            EventTime = eventTime;
            MessageId = id;
            IsMapSpecific = isMapSpecific;
        }

        public bool IsAnnounced()
        {
            return Announced;
        }

        public long GetEventTime()
        {
            return EventTime;
        }

        public void Execute()
        {
            PacketNotifier.notifyAnnounceEvent(MessageId, IsMapSpecific);
            Announced = true;
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
