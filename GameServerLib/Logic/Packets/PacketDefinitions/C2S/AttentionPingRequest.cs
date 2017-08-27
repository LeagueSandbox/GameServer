using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class AttentionPingRequest : ClientPacketBase
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public uint TargetNetId { get; private set; }
        public Pings Type { get; private set; }

        public AttentionPingRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            TargetNetId = reader.ReadUInt32();
            Type = (Pings)reader.ReadByte();
        }
    }
}