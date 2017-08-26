using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class ViewRequest : ClientPacketBase
    {
        public float X { get; private set; }
        public float Zoom { get; private set; }
        public float Y { get; private set; }
        public float Y2 { get; private set; }       //Unk
        public int Width { get; private set; }  //Unk
        public int Height { get; private set; } //Unk
        public int Unk2 { get; private set; }   //Unk
        public byte RequestNo { get; private set; }

        public ViewRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Zoom = reader.ReadSingle();
            Y = reader.ReadSingle();
            Y2 = reader.ReadSingle();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Unk2 = reader.ReadInt32();
            RequestNo = reader.ReadByte();
        }
    }
}