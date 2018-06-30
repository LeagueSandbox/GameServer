using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BeginAutoAttack : BasePacket
    {
        public BeginAutoAttack(AttackableUnit attacker, AttackableUnit attacked, uint futureProjNetId, bool isCritical)
            : base(PacketCmd.PKT_S2C_BEGIN_AUTO_ATTACK, attacker.NetId)
        {
            _buffer.Write((uint)attacked.NetId);
            _buffer.Write((byte)0x80); // extraTime
            _buffer.Write((uint)futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
            {
                _buffer.Write((byte)0x49); // attackSlot
            }
            else
            {
                _buffer.Write((byte)0x40); // attackSlot
            }

            _buffer.Write((byte)0x80); // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            _buffer.Write((byte)0x01);
            _buffer.Write(MovementVector.TargetXToNormalFormat(attacked.X));
            _buffer.Write((byte)0x80);
            _buffer.Write((byte)0x01);
            _buffer.Write(MovementVector.TargetYToNormalFormat(attacked.Y));
            _buffer.Write((byte)0xCC);
            _buffer.Write((byte)0x35);
            _buffer.Write((byte)0xC4);
            _buffer.Write((byte)0xD1);
            _buffer.Write((float)attacker.X);
            _buffer.Write((float)attacker.Y);
        }
    }
}