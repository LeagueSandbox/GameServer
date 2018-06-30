using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnAzirTurret : BasePacket
    {
        public SpawnAzirTurret(AzirTurret turret)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, turret.NetId)
        {
            _buffer.Write((byte)0xAD);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0xAB);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0xFE);

            _buffer.Write(turret.NetId);
            _buffer.Write((byte)0x23);
            _buffer.Write((byte)0x01);
            _buffer.Write(turret.NetId);
            _buffer.Write(turret.Owner.NetId);

            _buffer.Write((byte)0x40);

            _buffer.Write(Encoding.Default.GetBytes(turret.Name));
            _buffer.Fill(0, 64 - turret.Name.Length);

            _buffer.Write(Encoding.Default.GetBytes(turret.Model));
            _buffer.Fill(0, 64 - turret.Model.Length);

            _buffer.Write(0);

            _buffer.Write(turret.X);
            _buffer.Write(turret.GetZ());
            _buffer.Write(turret.Y);
            _buffer.Write(4.0f);

            _buffer.Write((byte)0xC1);
            _buffer.Write((short)turret.Team);

            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x02);

            _buffer.Fill(0, 11);

            _buffer.Write(1.0f); // Unk

            _buffer.Fill(0, 13);
        }
    }
}