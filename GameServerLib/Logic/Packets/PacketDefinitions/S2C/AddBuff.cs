using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddBuff : BasePacket
    {
        public AddBuff(AttackableUnit u, AttackableUnit source, int stacks, float time, BuffType buffType, string name, byte slot)
            : base(PacketCmd.PKT_S2_C_ADD_BUFF, u.NetId)
        {
            _buffer.Write((byte)slot); //Slot
            _buffer.Write((byte)buffType); //Type
            _buffer.Write((byte)stacks); // stacks
            _buffer.Write((byte)0x00); // Visible was (byte)0x00
            _buffer.Write(HashFunctions.HashString(name)); //Buff id

            _buffer.Write((int)0); // <-- Probably runningTime

            _buffer.Write((float)0); // <- ProgressStartPercent

            _buffer.Write((float)time);

            if (source != null)
            {
                _buffer.Write(source.NetId); //source
            }
            else
            {
                _buffer.Write((int)0);
            }
        }
    }
}