using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SWAP_ITEMS;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new SwapItemsRequest(data);
            if (request.SlotFrom > 6 || request.SlotTo > 6)
            {
                return false;
            }

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            PlayerManager.GetPeerInfo(peer).Champion.GetInventory().SwapItems(request.SlotFrom, request.SlotTo);
            Game.PacketNotifier.NotifyItemsSwapped(
                PlayerManager.GetPeerInfo(peer).Champion,
                request.SlotFrom,
                request.SlotTo
            );

            return true;
        }
    }
}
