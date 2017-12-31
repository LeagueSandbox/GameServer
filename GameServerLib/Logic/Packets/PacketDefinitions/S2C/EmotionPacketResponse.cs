using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EmotionPacketResponse : BasePacket
    {
        public EmotionPacketResponse(Emotions id, uint netId)
            : base(PacketCmd.PKT_S2C_Emotion, netId)
        {
            buffer.Write((byte)id);
        }
    }
}