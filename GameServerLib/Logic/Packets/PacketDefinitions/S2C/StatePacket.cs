using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StatePacket : BasePacket
    {
        public StatePacket(Game game, PacketCmd state)
            : base(game, state)
        {

        }
    }
}