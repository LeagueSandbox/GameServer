using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class AttentionPingRequest
    {
        public byte Cmd;
        public int Unk1;
        public float X;
        public float Y;
        public int TargetNetId;
        public Pings Type;

        public AttentionPingRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = reader.ReadByte();
                Unk1 = reader.ReadInt32();
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                TargetNetId = reader.ReadInt32();
                Type = (Pings)reader.ReadByte();
            }
        }

        public AttentionPingRequest(float x, float y, int netId, Pings type)
        {
            X = x;
            Y = y;
            TargetNetId = netId;
            Type = type;
        }
    }
}