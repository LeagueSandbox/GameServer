using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class PingLoadInfoRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public int Position;
        public long UserId;
        public float Loaded;
        public float Unk2;
        public short Ping;
        public short Unk3;
        public byte Unk4;

        public PingLoadInfoRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadUInt32();
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
}