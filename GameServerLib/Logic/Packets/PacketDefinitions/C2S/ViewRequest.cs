using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class ViewRequest
    {
        public PacketCmd cmd;
        public int netId;
        public float x;
        public float zoom;
        public float y;
        public float y2;       //Unk
        public int width;  //Unk
        public int height; //Unk
        public int unk2;   //Unk
        public byte requestNo;

        public ViewRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            x = reader.ReadSingle();
            zoom = reader.ReadSingle();
            y = reader.ReadSingle();
            y2 = reader.ReadSingle();
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            unk2 = reader.ReadInt32();
            requestNo = reader.ReadByte();

            reader.Close();
        }
    }
}