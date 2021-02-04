using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class CastSpellRequest : ICoreRequest
    {
        public byte SpellSlot { get; }
        public bool IsSummonerSpellBook { get; }
        public bool IsHudClickCast { get; }
        public Vector2 Position { get; }
        public Vector2 EndPosition { get; }
        // TODO: change type to IAttackableUnit
        public uint TargetNetId { get; }

        public CastSpellRequest(byte spellSlot, bool isSummonerSpellBook, bool isHudClickCast, Vector2 start, Vector2 end, uint targetNetId)
        {
            SpellSlot = spellSlot;
            IsSummonerSpellBook = isSummonerSpellBook;
            IsHudClickCast = isHudClickCast;
            Position = start;
            EndPosition = end;
            TargetNetId = targetNetId;
        }
    }
}
