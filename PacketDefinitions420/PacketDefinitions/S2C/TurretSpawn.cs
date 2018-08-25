using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(IBaseTurret t)
            : base(PacketCmd.PKT_S2C_TURRET_SPAWN, t.ParentNetId)
        {
            WriteNetId(t);
            Write((byte)0x40);
			WriteConstLengthString(t.Name, 64);
            Write((byte)0x0C);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x80);
            Write((byte)0x01);
        }
    }
}