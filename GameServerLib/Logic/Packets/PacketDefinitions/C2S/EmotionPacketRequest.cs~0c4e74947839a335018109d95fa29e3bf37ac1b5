using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class EmotionPacketRequest : ClientPacketBase
    {
        public Emotions EmoteId { get; private set; }

        public EmotionPacketRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            EmoteId = (Emotions)reader.ReadByte();
        }
    }
}