using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SpellChargeUpdateReq : ICoreRequest
    {
        public byte Slot { get; }
        public bool IsSummonerSpellBook { get; }
        public bool ForceStop { get; }
        public Vector3 Position { get; }

        public SpellChargeUpdateReq(byte slot, bool isSummonerSpellBook, Vector3 position, bool forceStop = false)
        {
            Slot = slot;
            IsSummonerSpellBook = isSummonerSpellBook;
            ForceStop = forceStop;
            Position = position;
        }
    }
}
