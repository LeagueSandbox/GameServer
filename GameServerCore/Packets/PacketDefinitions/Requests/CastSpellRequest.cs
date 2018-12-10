namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class CastSpellRequest : ICoreRequest
    {
        public int NetId { get; }
        // TODO: use Spell class instead of the slot
        public byte SpellSlot { get; }
        // TODO: change to 2 Vector2D, start and end
        public float X { get; }
        public float Y { get; }
        public float X2 { get; }
        public float Y2 { get; }
        // TODO: change type to IAttackableUnit
        public uint TargetNetId { get; }

        public CastSpellRequest(int netId, byte spellSlot, float x, float y, float x2, float y2, uint targetNetId)
        {
            NetId = netId;
            SpellSlot = spellSlot;
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            TargetNetId = targetNetId;
        }
    }
}
