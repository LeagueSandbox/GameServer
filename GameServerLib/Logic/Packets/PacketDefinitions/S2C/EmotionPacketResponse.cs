using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EmotionPacketResponse : BasePacket
    {
        public PacketCmd Cmd;
        public uint NetId;
        public byte Id;

        public EmotionPacketResponse(Game game, byte id, uint netId) 
            : base(game, PacketCmd.PKT_S2C_EMOTION, netId)
        {
            Write(id);
        }
    }
}