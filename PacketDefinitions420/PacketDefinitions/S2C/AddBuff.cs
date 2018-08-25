using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class AddBuff : BasePacket
    {
        public AddBuff(IAttackableUnit u, IAttackableUnit source, int stacks, float time, BuffType buffType, string name, byte slot)
            : base(PacketCmd.PKT_S2C_ADD_BUFF, u.NetId)
        {
            Write((byte)slot); //Slot
            Write((byte)buffType); //Type
            Write((byte)stacks); // stacks
            Write((byte)0x00); // Visible was (byte)0x00
            WriteStringHash(name); //Buff id

            Write((int)0); // <-- Probably runningTime

            Write((float)0); // <- ProgressStartPercent

            Write((float)time);

            WriteNetId(source);
        }
    }
}