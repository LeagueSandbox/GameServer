namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class ViewRequest : ICoreRequest
    {
        public int NetId { get; }
        public float X { get; }
        public float Zoom { get; }
        public float Y { get; }
        public float Y2 { get; }       //Unk
        public int Width { get; }  //Unk
        public int Height { get; } //Unk
        public int Unk2 { get; }   //Unk
        public byte RequestNo { get; }

        public ViewRequest(int netId, float x, float zoom, float y, float y2, int width, int height, int unk2, byte requestNo)
        {
            NetId = netId;
            X = x;
            Zoom = zoom;
            Y = y;
            Y2 = y2;
            Width = width;
            Height = height;
            Unk2 = unk2;
            RequestNo = requestNo;
        }
    }
}
