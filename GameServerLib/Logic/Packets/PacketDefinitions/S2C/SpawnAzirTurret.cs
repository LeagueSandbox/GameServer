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
            Write((byte)0xAD);
            Write((byte)0x00);
            Write((byte)0xAB);
            Write((byte)0x00);
            Write((byte)0xFE);

            Write(turret.NetId);
            Write((byte)0x23);
            Write((byte)0x01);
            Write(turret.NetId);
            Write(turret.Owner.NetId);

            Write((byte)0x40);

            Write(Encoding.Default.GetBytes(turret.Name));
            Fill(0, 64 - turret.Name.Length);

            Write(Encoding.Default.GetBytes(turret.Model));
            Fill(0, 64 - turret.Model.Length);

            Write(0);

            Write(turret.X);
            Write(turret.GetZ());
            Write(turret.Y);
            Write(4.0f);

            Write((byte)0xC1);
            Write((short)turret.Team);

            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x02);

            Fill(0, 11);

            Write(1.0f); // Unk

            Fill(0, 13);
        }
    }
}