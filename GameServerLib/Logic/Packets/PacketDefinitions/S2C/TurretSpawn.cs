using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(BaseTurret t)
            : base(PacketCmd.PKT_S2C_TurretSpawn)
        {
            buffer.Write((int)t.NetId);
            buffer.Write((byte)0x40);
            foreach (var b in Encoding.Default.GetBytes(t.Name))
                buffer.Write((byte)b);
            buffer.fill(0, 64 - t.Name.Length);
            buffer.Write((byte)0x0C);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
        }
    }
}