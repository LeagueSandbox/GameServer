using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(BaseTurret t)
            : base(PacketCmd.PKT_S2_C_TURRET_SPAWN, t.ParentNetId)
        {
            _buffer.Write((int)t.NetId);
            _buffer.Write((byte)0x40);
            foreach (var b in Encoding.Default.GetBytes(t.Name))
                _buffer.Write(b);
            _buffer.Fill(0, 64 - t.Name.Length);
            _buffer.Write((byte)0x0C);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x80);
            _buffer.Write((byte)0x01);
        }
    }
}