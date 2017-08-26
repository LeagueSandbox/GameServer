using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class QuestClicked
    {
        public byte cmd;
        public uint playernetid;
        public byte unk;
        public uint netid;

        public QuestClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            playernetid = reader.ReadUInt32();
            unk = reader.ReadByte();
            netid = reader.ReadUInt32();
        }
        public QuestClicked()
        {

        }
    }
}