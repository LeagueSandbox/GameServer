using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class PingLoadInfoRequest : ClientPacketBase
    {
        public int Position { get; private set; }
        public long UserId { get; private set; }
        public float Loaded { get; private set; }
        public float Unk2 { get; private set; }
        public short Ping { get; private set; }
        public short Unk3 { get; private set; }
        public byte Unk4 { get; private set; }

        public PingLoadInfoRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Position = reader.ReadInt32();
            UserId = reader.ReadInt64();
            Loaded = reader.ReadSingle();
            Unk2 = reader.ReadSingle();
            Ping = reader.ReadInt16();
            Unk3 = reader.ReadInt16();
            Unk4 = reader.ReadByte();
        }
    }
}