using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BlueTipClickedRequest : ICoreRequest
    {
        public TipCommand TipCommand { get; set; }
        public uint TipID { get; set; }

        public BlueTipClickedRequest(TipCommand tipCommand, uint tipId)
        {
            TipCommand = tipCommand;
            TipID = tipId;
        }
    }
}
