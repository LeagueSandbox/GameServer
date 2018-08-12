using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class AutoAttackOption
    {
        public byte cmd;
        public int netid;
        public byte activated;

        public AutoAttackOption(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            netid = reader.ReadInt32();
            activated = reader.ReadByte();
        }
        public AutoAttackOption()
        {

        }
    }
}