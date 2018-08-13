using System.IO;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class BasicTutorialMessageWindowClicked
    {
        public byte Cmd;
        public int Unk;

        public BasicTutorialMessageWindowClicked(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = reader.ReadByte();
                Unk = reader.ReadInt32(); // Seems to be always 0
            }
        }

        public BasicTutorialMessageWindowClicked()
        {

        }
    }
}