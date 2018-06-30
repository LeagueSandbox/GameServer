using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(PacketCmd state)
            : base(state)
        {
            _buffer.Write((short)0); //unk
        }
    }
}