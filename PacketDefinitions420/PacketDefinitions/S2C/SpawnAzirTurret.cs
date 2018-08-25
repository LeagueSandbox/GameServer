using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpawnAzirTurret : BasePacket
    {
        public SpawnAzirTurret(IAzirTurret turret)
            : base(PacketCmd.PKT_S2C_OBJECT_SPAWN, turret.NetId)
        {
            Write((byte)0xAD);
            Write((byte)0x00);
            Write((byte)0xAB);
            Write((byte)0x00);
            Write((byte)0xFE);

            WriteNetId(turret);
            Write((byte)0x23);
            Write((byte)0x01);
            WriteNetId(turret);
            WriteNetId(turret.Owner);

            Write((byte)0x40);

			WriteConstLengthString(turret.Name, 64);

			WriteConstLengthString(turret.Model, 64);

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