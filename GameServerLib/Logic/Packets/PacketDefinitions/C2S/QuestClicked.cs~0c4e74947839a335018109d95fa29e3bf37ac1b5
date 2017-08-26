using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class QuestClicked : ClientPacketBase
    {
        public byte Unk { get; private set; }
        public uint QuestNetId { get; private set; }

        public QuestClicked(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Unk = reader.ReadByte();
            QuestNetId = reader.ReadUInt32();
        }
    }
}