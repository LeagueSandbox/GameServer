using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyParticle : BasePacket
    {
        public DestroyParticle(Particle p)
            : base(PacketCmd.PKT_S2_C_DESTROY_OBJECT, p.NetId)
        {
            _buffer.Write(p.NetId);
        }
    }
}