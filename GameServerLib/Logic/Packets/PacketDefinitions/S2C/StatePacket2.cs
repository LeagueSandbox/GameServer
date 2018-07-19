using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(Game game, PacketCmd state)
            : base(game, state)
        {
            Write((short)0); //unk
        }
    }
}