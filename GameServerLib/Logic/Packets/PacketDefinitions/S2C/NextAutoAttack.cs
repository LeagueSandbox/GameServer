using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NextAutoAttack : BasePacket
    {
        public NextAutoAttack(Game game, AttackableUnit attacker, AttackableUnit attacked, uint futureProjNetId, bool isCritical, bool initial)
            : base(game, PacketCmd.PKT_S2C_NEXT_AUTO_ATTACK, attacker.NetId)
        {
            WriteNetId(attacked);
            if (initial)
                Write((byte)0x80); // extraTime
            else
                Write((byte)0x7F); // extraTime

            Write(futureProjNetId);
            if (isCritical)
                Write((byte)0x49); // attackSlot
            else
                Write((byte)0x40); // attackSlot

            Write((byte)0x40);
            Write((byte)0x01);
            Write((byte)0x7B);
            Write((byte)0xEF);
            Write((byte)0xEF);
            Write((byte)0x01);
            Write((byte)0x2E);
            Write((byte)0x55);
            Write((byte)0x55);
            Write((byte)0x35);
            Write((byte)0x94);
            Write((byte)0xD3);
        }
    }
}