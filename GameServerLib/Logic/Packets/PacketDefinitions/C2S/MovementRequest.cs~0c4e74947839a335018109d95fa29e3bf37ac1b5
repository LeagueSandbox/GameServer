using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class MovementRequest : ClientPacketBase
    {
        public MoveType MovementType { get; private set; } //byte
        public float X { get; private set; }
        public float Y { get; private set; }
        public uint TargetNetId { get; private set; }
        public byte CoordCount { get; private set; }
        public int NetIdPacket { get; private set; }
        public byte[] moveData { get; private set; }

        public MovementRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            MovementType = (MoveType)reader.ReadByte();
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            TargetNetId = reader.ReadUInt32();
            CoordCount = reader.ReadByte();
            NetIdPacket = reader.ReadInt32();
            moveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }
    }
}