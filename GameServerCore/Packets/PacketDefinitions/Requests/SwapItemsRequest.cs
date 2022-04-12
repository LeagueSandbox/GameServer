namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SwapItemsRequest : ICoreRequest
    {
        public byte Source { get; }
        public byte Destination { get; }

        public SwapItemsRequest(byte source, byte destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
