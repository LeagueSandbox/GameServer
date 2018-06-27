using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NextAutoAttack : BasePacket
    {
        public NextAutoAttack(AttackableUnit attacker, AttackableUnit attacked, uint futureProjNetId, bool isCritical, bool initial)
            : base(PacketCmd.PKT_S2C_NEXT_AUTO_ATTACK, attacker.NetId)
        {
            _buffer.Write(attacked.NetId);
            if (initial)
                _buffer.Write((byte)0x80); // extraTime
            else
                _buffer.Write((byte)0x7F); // extraTime

            _buffer.Write(futureProjNetId);
            if (isCritical)
                _buffer.Write((byte)0x49); // attackSlot
            else
                _buffer.Write((byte)0x40); // attackSlot

            _buffer.Write((byte)0x40);
            _buffer.Write((byte)0x01);
            _buffer.Write((byte)0x7B);
            _buffer.Write((byte)0xEF);
            _buffer.Write((byte)0xEF);
            _buffer.Write((byte)0x01);
            _buffer.Write((byte)0x2E);
            _buffer.Write((byte)0x55);
            _buffer.Write((byte)0x55);
            _buffer.Write((byte)0x35);
            _buffer.Write((byte)0x94);
            _buffer.Write((byte)0xD3);
        }
    }
}