using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BlueTipClicked
    {
        public byte cmd;
        public uint playernetid;
        public byte unk;
        public uint netid;

        public BlueTipClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            playernetid = reader.ReadUInt32();
            unk = reader.ReadByte();
            netid = reader.ReadUInt32();
        }
        public BlueTipClicked()
        {

        }
    }
}