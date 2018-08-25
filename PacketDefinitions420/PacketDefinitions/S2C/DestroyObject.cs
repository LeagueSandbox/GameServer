using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class DestroyObject : BasePacket
    {
        public DestroyObject(IAttackableUnit destroyer, IAttackableUnit destroyed)
            : base(PacketCmd.PKT_S2C_DESTROY_OBJECT, destroyer.NetId)
        {
            WriteNetId(destroyed);
        }
    }
}