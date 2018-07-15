using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class MovementRequest
    {
        public PacketCmd Cmd;
        public int NetIdHeader;
        public MoveType Type; //byte
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
                Type = (MoveType)reader.ReadByte();
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