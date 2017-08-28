using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BeginAutoAttack : BasePacket
    {
        public BeginAutoAttack(BeginAutoAttackArgs args)
            : base(PacketCmd.PKT_S2C_BeginAutoAttack, args.Attacker.UnitNetId)
        {
            buffer.Write(args.Attacked.UnitNetId);
            buffer.Write((byte)0x80); // extraTime
            buffer.Write(args.FutureProjNetId); // Basic attack projectile ID, to be spawned later
            if (args.IsCritical)
                buffer.Write((byte)0x49); // attackSlot
            else
                buffer.Write((byte)0x40); // attackSlot

            buffer.Write((byte)0x80); // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetXToNormalFormat(args.Attacked.X));
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetYToNormalFormat(args.Attacked.Y));
            buffer.Write((byte)0xCC);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0xC4);
            buffer.Write((byte)0xD1);
            buffer.Write(args.Attacker.X);
            buffer.Write(args.Attacker.Y);
        }
    }
}