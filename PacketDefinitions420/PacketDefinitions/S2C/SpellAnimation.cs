using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpellAnimation : BasePacket
    {
        public SpellAnimation(IAttackableUnit u, string animationName)
            : base(PacketCmd.PKT_S2C_SPELL_ANIMATION, u.NetId)
        {
            Write((byte)0xC4); // unk  <--
            Write((uint)0); // unk     <-- One of these bytes is a flag
            Write((uint)0); // unk     <--
            Write(1.0f); // Animation speed scale factor
			Write(animationName);
            Write((byte)0);
        }
    }
}