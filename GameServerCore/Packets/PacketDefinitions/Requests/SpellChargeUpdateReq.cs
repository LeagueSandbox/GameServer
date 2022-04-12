using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SpellChargeUpdateReq : ICoreRequest
    {
        public byte Slot { get; set; }
        public bool IsSummonerSpellBook { get; set; }
        public bool ForceStop { get; set; }
        public Vector3 Position { get; set; }

        public SpellChargeUpdateReq(byte slot, bool isSummonerSpellBook, Vector3 position, bool forceStop = false)
        {
            Slot = slot;
            IsSummonerSpellBook = isSummonerSpellBook;
            ForceStop = forceStop;
            Position = position;
        }
    }
}
