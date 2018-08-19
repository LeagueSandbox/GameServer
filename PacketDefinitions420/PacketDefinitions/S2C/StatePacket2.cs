using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(PacketCmd state)
            : base(state)
        {
            Write((short)0); //unk
        }
    }
}