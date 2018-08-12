using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpawnAzirTurret : BasePacket
    {
        public SpawnAzirTurret(AzirTurret turret) 
            : base(PacketCmd.PKT_S2C_ObjectSpawn, turret.NetId)
        {
            buffer.Write((byte)0xAD);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xAB);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xFE);

            buffer.Write(turret.NetId);
            buffer.Write((byte)0x23);
            buffer.Write((byte)0x01);
            buffer.Write(turret.NetId);
            buffer.Write(turret.Owner.NetId);

            buffer.Write((byte)0x40);

            buffer.Write(Encoding.Default.GetBytes(turret.Name));
            buffer.fill(0, 64 - turret.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(turret.Model));
            buffer.fill(0, 64 - turret.Model.Length);

            buffer.Write((int)0);

            buffer.Write((float)turret.X);
            buffer.Write((float)turret.GetZ());
            buffer.Write((float)turret.Y);
            buffer.Write((float)4.0f);

            buffer.Write((byte)0xC1);
            buffer.Write((short)turret.Team);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);

            buffer.fill(0, 11);

            buffer.Write((float)1.0f); // Unk

            buffer.fill(0, 13);
        }
    }
}