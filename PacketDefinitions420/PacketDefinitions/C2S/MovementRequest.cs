using System.IO;
using GameServerCore.Packets.Enums;
using PacketDefinitions420.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class MovementRequest
    {
        public PacketCmd Cmd;
        public int NetIdHeader;
        public MovementType Type; //byte
        public float X;
        public float Y;
        public uint TargetNetId;
        public byte CoordCount;
        public int NetId;
        public byte[] MoveData;

        public MovementRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetIdHeader = reader.ReadInt32();
                Type = (MovementType)reader.ReadByte();
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                TargetNetId = reader.ReadUInt32();
                CoordCount = reader.ReadByte();
                NetId = reader.ReadInt32();
                MoveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }
        }
    }
}