using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BlueTipClickedRequest : ICoreRequest
    {
        public TipCommand TipCommand { get; }
        public uint TipID { get; }

        public BlueTipClickedRequest(TipCommand tipCommand, uint tipId)
        {
            TipCommand = tipCommand;
            TipID = tipId;
        }
    }
}
