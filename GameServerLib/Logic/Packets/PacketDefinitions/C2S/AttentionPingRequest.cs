using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class AttentionPingRequest
    {
        public byte cmd;
        public int unk1;
        public float x;
        public float y;
        public int targetNetId;
        public Pings type;

        public AttentionPingRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            unk1 = reader.ReadInt32();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            targetNetId = reader.ReadInt32();
            type = (Pings)reader.ReadByte();
        }
        
        public AttentionPingRequest(float x, float y, int netId, Pings type)
        {
            this.x = x;
            this.y = y;
            this.targetNetId = netId;
            this.type = type;
        }
    }
}