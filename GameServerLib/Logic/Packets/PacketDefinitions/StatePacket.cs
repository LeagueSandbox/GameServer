using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class StatePacket : BasePacket
    {
        public StatePacket(PacketCmd state) : base(state)
        {

        }
    }
}