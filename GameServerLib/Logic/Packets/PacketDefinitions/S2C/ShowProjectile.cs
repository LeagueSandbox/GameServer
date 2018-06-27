using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ShowProjectile : BasePacket
    {
        public ShowProjectile(Projectile p)
            : base(PacketCmd.PKT_S2_C_SHOW_PROJECTILE, p.Owner.NetId)
        {
            _buffer.Write(p.NetId);
        }
    }
}