using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpellAnimation : BasePacket
    {
        public SpellAnimation(AttackableUnit u, string animationName)
            : base(PacketCmd.PKT_S2_C_SPELL_ANIMATION, u.NetId)
        {
            _buffer.Write((byte)0xC4); // unk  <--
            _buffer.Write((uint)0); // unk     <-- One of these bytes is a flag
            _buffer.Write((uint)0); // unk     <--
            _buffer.Write((float)1.0f); // Animation speed scale factor
            foreach (var b in Encoding.Default.GetBytes(animationName))
                _buffer.Write(b);
            _buffer.Write((byte)0);
        }
    }
}