using System.IO;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class QuestClicked
    {
        public byte Cmd;
        public uint Playernetid;
        public byte Unk;
        public uint Netid;

        public QuestClicked(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = reader.ReadByte();
                Playernetid = reader.ReadUInt32();
                Unk = reader.ReadByte();
                Netid = reader.ReadUInt32();
            }
        }
    }
}