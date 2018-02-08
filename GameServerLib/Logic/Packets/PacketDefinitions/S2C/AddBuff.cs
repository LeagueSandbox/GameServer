using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddBuff : BasePacket
    {
        public AddBuff(AttackableUnit u, AttackableUnit source, int stacks, float time, BuffType buffType, string name, byte slot)
            : base(PacketCmd.PKT_S2C_AddBuff, u.NetId)
        {
            buffer.Write((byte)slot); //Slot
            buffer.Write((byte)buffType); //Type
            buffer.Write((byte)stacks); // stacks
            buffer.Write((byte)0x00); // Visible was (byte)0x00
            buffer.Write(HashFunctions.HashString(name)); //Buff id

            buffer.Write((int)0); // <-- Probably runningTime

            buffer.Write((float)0); // <- ProgressStartPercent

            buffer.Write((float)time);

            if (source != null)
            {
                buffer.Write(source.NetId); //source
            }
            else
            {
                buffer.Write((int)0);
            }
        }
    }
}