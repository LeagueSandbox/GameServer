using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class DestroyProjectile : BasePacket
    {
        public DestroyProjectile(IProjectile p)
            : base(PacketCmd.PKT_S2C_DESTROY_PROJECTILE, p.NetId)
        {

        }
    }
}