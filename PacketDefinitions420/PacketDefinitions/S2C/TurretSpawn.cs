using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(BaseTurret t)
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