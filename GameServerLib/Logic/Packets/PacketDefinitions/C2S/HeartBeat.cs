using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class HeartBeat
    {
        public PacketCmd cmd;
        public int netId;
        public float receiveTime;
        public float ackTime;
        public HeartBeat(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            receiveTime = reader.ReadSingle();
            ackTime = reader.ReadSingle();
            reader.Close();
        }
    }
}