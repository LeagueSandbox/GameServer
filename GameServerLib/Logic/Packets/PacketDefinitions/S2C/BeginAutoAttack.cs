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
            WriteNetId(attacked);
            Write((byte)0x80); // extraTime
            Write((uint)futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
            {
                Write((byte)0x49); // attackSlot
            }
            else
            {
                Write((byte)0x40); // attackSlot
            }

            Write((byte)0x80); // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            Write((byte)0x01);
            Write(MovementVector.TargetXToNormalFormat(attacked.X));
            Write((byte)0x80);
            Write((byte)0x01);
            Write(MovementVector.TargetYToNormalFormat(attacked.Y));
            Write((byte)0xCC);
            Write((byte)0x35);
            Write((byte)0xC4);
            Write((byte)0xD1);
            Write((float)attacker.X);
            Write((float)attacker.Y);
        }
    }
}