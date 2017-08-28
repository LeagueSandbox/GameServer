using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DamageDone : BasePacket
    {
        public DamageDone(DamageDoneArgs args)
            : base(PacketCmd.PKT_S2C_DamageDone, args.DamageTargetNetId)
        {
            buffer.Write((byte)args.DamageText);
            buffer.Write((short)((short)args.Type << 8));
            buffer.Write((float)args.Amount);
            buffer.Write((uint)args.DamageTargetNetId);
            buffer.Write((uint)args.DamageSourceNetId);
        }
    }
}