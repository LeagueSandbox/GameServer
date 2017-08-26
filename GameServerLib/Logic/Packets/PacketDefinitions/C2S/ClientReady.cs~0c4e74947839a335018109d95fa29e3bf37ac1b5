using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class ClientReady : ClientPacketBase
    {
        public int TeamId { get; private set; }

        public ClientReady(byte[] data) : base(data)
        {
        }

        protected override void ParseInternal(BinaryReader reader)
        {
            TeamId = reader.ReadInt32();
        }
    }
}