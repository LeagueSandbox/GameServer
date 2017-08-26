using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class HeartBeat : ClientPacketBase
    {
        public float ReceiveTime { get; private set; }
        public float AckTime { get; private set; }

        public HeartBeat(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            ReceiveTime = reader.ReadSingle();
            AckTime = reader.ReadSingle();
        }
    }
}