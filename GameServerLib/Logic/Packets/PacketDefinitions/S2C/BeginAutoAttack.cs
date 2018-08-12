using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BeginAutoAttack : BasePacket
    {
        public BeginAutoAttack(AttackableUnit attacker, AttackableUnit attacked, uint futureProjNetId, bool isCritical)
            : base(PacketCmd.PKT_S2C_BeginAutoAttack, attacker.NetId)
        {
            buffer.Write(attacked.NetId);
            buffer.Write((byte)0x80); // extraTime
            buffer.Write(futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
                buffer.Write((byte)0x49); // attackSlot
            else
                buffer.Write((byte)0x40); // attackSlot

            buffer.Write((byte)0x80); // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetXToNormalFormat(attacked.X));
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetYToNormalFormat(attacked.Y));
            buffer.Write((byte)0xCC);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0xC4);
            buffer.Write((byte)0xD1);
            buffer.Write(attacker.X);
            buffer.Write(attacker.Y);
        }
    }
}