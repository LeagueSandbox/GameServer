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
            Write((int)t.NetId);
            Write((byte)0x40);
            foreach (var b in Encoding.Default.GetBytes(t.Name))
                Write(b);
            Fill(0, 64 - t.Name.Length);
            Write((byte)0x0C);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x80);
            Write((byte)0x01);
        }
    }
}