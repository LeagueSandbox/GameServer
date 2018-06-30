using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StatePacket : BasePacket
    {
        public StatePacket(PacketCmd state)
            : base(state)
        {

        }
    }
}