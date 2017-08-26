using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyParticle : BasePacket
    {
        public DestroyParticle(Particle p)
            : base(PacketCmd.PKT_S2C_DestroyObject, p.NetId)
        {
            buffer.Write((uint)p.NetId);
        }
    }
}