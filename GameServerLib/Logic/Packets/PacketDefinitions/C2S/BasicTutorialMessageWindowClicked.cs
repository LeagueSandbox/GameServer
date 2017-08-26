using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BasicTutorialMessageWindowClicked
    {
        public byte cmd;
        public int unk;

        public BasicTutorialMessageWindowClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            unk = reader.ReadInt32(); // Seems to be always 0
        }
        public BasicTutorialMessageWindowClicked()
        {

        }
    }
}