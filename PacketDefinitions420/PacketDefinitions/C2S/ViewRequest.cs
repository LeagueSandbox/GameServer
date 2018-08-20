using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class ViewRequest
    {
        public PacketCmd Cmd;
        public int NetId;
        public float X;
        public float Zoom;
        public float Y;
        public float Y2;       //Unk
        public int Width;  //Unk
        public int Height; //Unk
        public int Unk2;   //Unk
        public byte RequestNo;

        public ViewRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadInt32();
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
}