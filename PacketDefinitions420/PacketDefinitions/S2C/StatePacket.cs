using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class StatePacket : BasePacket
    {
        public StatePacket(PacketCmd state)
            : base(state)
        {

        }
    }
}