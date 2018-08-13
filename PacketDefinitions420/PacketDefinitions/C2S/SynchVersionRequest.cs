using System.IO;
using System.Text;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class SynchVersionRequest
    {
        public PacketCmd Cmd;
        public int NetId;
        public int Unk1;
        private byte[] _version = new byte[256]; // version string might be shorter?
        public string Version
        {
            get
            {
                var s = Encoding.Default.GetString(_version);
                var idx = s.IndexOf('\0');
                if (idx > 0)
                {
                    return s.Substring(0, idx);
                }

                return s;
            }
        }

        public SynchVersionRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadInt32();
                Unk1 = reader.ReadInt32();
                _version = reader.ReadBytes(256);
            }
        }
    }
}