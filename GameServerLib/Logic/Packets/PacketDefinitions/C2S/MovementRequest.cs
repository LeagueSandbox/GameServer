using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class MovementRequest
    {
        public PacketCmd cmd;
        public int netIdHeader;
        public MoveType type; //byte
        public float x;
        public float y;
        public uint targetNetId;
        public byte coordCount;
        public int netId;
        public byte[] moveData;

        public MovementRequest(byte[] data)
        {
            var baseStream = new MemoryStream(data);
            var reader = new BinaryReader(baseStream);
            cmd = (PacketCmd)reader.ReadByte();
            netIdHeader = reader.ReadInt32();
            type = (MoveType)reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            targetNetId = reader.ReadUInt32();
            coordCount = reader.ReadByte();
            netId = reader.ReadInt32();
            moveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            reader.Close();
        }
    }
}