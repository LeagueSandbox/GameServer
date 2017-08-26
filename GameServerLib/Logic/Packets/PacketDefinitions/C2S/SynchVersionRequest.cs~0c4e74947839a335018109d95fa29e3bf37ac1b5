using System.IO;
using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SynchVersionRequest : ClientPacketBase
    {
        public int Unk1 { get; private set; }
        public string Version { get; private set; }

        public SynchVersionRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Unk1 = reader.ReadInt32();
            var version = reader.ReadBytes(256);

            Version = GetVersionString(version);
        }

        private static string GetVersionString(byte[] version)
        {
            if (version == null)
                return string.Empty;

            var s = Encoding.Default.GetString(version);
            var idx = s.IndexOf('\0');
            if (idx > 0)
                return s.Substring(0, idx);

            return s;
        }
    }
}