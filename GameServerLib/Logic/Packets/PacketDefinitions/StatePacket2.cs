using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(PacketCmd state) : base(state)
        {
            buffer.Write((short)0); //unk
        }
    }
}