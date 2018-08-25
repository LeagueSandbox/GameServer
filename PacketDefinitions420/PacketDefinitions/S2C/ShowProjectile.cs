using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ShowProjectile : BasePacket
    {
        public ShowProjectile(IProjectile p)
            : base(PacketCmd.PKT_S2C_SHOW_PROJECTILE, p.Owner.NetId)
        {
            WriteNetId(p);
        }
    }
}