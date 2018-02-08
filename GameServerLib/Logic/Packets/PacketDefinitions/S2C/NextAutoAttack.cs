using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NextAutoAttack : BasePacket
    {
        public NextAutoAttack(AttackableUnit attacker, AttackableUnit attacked, uint futureProjNetId, bool isCritical, bool initial)
            : base(PacketCmd.PKT_S2C_NextAutoAttack, attacker.NetId)
        {
            buffer.Write(attacked.NetId);
            if (initial)
                buffer.Write((byte)0x80); // extraTime
            else
                buffer.Write((byte)0x7F); // extraTime

            buffer.Write(futureProjNetId);
            if (isCritical)
                buffer.Write((byte)0x49); // attackSlot
            else
                buffer.Write((byte)0x40); // attackSlot

            buffer.Write((byte)0x40);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x7B);
            buffer.Write((byte)0xEF);
            buffer.Write((byte)0xEF);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x2E);
            buffer.Write((byte)0x55);
            buffer.Write((byte)0x55);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0x94);
            buffer.Write((byte)0xD3);
        }
    }
}