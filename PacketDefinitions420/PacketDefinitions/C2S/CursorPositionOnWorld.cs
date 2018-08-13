using System.IO;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class CursorPositionOnWorld
    {
        public byte Cmd;
        public uint NetId;
        public short Unk1; // Maybe 2 bytes instead of 1 short?
        public float X;
        public float Z;
        public float Y;
        public CursorPositionOnWorld(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = reader.ReadByte();
                NetId = reader.ReadUInt32();
                Unk1 = reader.ReadInt16();
                X = reader.ReadSingle();
                Z = reader.ReadSingle();
                Y = reader.ReadSingle();
            }
        }
    }
}