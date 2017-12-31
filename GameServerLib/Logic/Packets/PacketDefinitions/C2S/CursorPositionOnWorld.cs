using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class CursorPositionOnWorld : ClientPacketBase
    {
        public short Unk1 { get; private set; } // Maybe 2 bytes instead of 1 short?
        public float X { get; private set; }
        public float Z { get; private set; }
        public float Y { get; private set; }

        public CursorPositionOnWorld(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Unk1 = reader.ReadInt16();
            X = reader.ReadSingle();
            Z = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }
}