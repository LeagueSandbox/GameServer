using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpellAnimation : BasePacket
    {
        public SpellAnimation(Unit u, string animationName)
            : base(PacketCmd.PKT_S2C_SpellAnimation, u.NetId)
        {
            buffer.Write((byte)0xC4); // unk  <--
            buffer.Write((uint)0); // unk     <-- One of these bytes is a flag
            buffer.Write((uint)0); // unk     <--
            buffer.Write((float)1.0f); // Animation speed scale factor
            foreach (var b in Encoding.Default.GetBytes(animationName))
                buffer.Write(b);
            buffer.Write((byte)0);
        }
    }
}