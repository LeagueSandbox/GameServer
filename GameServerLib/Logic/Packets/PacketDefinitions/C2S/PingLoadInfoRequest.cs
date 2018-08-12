using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class PingLoadInfoRequest
    {
        public PacketCmd cmd;
        public uint netId;
        public int position;
        public long userId;
        public float loaded;
        public float unk2;
        public short ping;
        public short unk3;
        public byte unk4;

        public PingLoadInfoRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            position = reader.ReadInt32();
            userId = reader.ReadInt64();
            loaded = reader.ReadSingle();
            unk2 = reader.ReadSingle();
            ping = reader.ReadInt16();
            unk3 = reader.ReadInt16();
            unk4 = reader.ReadByte();
            reader.Close();
        }
    }
}