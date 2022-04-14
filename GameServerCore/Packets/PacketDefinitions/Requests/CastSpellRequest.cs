using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class CastSpellRequest : ICoreRequest
    {
        public byte Slot { get; }
        public bool IsSummonerSpellBook { get; }
        public bool IsHudClickCast { get; }
        public Vector2 Position { get; }
        public Vector2 EndPosition { get; }
        public uint TargetNetID { get; }

        public CastSpellRequest(byte spellSlot, bool isSummonerSpellBook, bool isHudClickCast, Vector2 start, Vector2 end, uint targetNetId)
        {
            Slot = spellSlot;
            IsSummonerSpellBook = isSummonerSpellBook;
            IsHudClickCast = isHudClickCast;
            Position = start;
            EndPosition = end;
            TargetNetID = targetNetId;
        }
    }
}
