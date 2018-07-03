using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EmotionPacketResponse : BasePacket
    {
        public PacketCmd Cmd;
        public uint NetId;
        public byte Id;

        public EmotionPacketResponse(byte id, uint netId) : base(PacketCmd.PKT_S2C_EMOTION, netId)
        {
            Write(id);
        }
    }
}