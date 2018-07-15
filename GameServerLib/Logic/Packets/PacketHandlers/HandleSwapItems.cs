using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SWAP_ITEMS;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleSwapItems(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new SwapItemsRequest(data);
            if (request.SlotFrom > 6 || request.SlotTo > 6)
            {
                return false;
            }

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            var champion = _playerManager.GetPeerInfo(peer).Champion;

            champion.Inventory.SwapItems(request.SlotFrom, request.SlotTo);
            champion.SwapSpells((byte)(6 + request.SlotFrom), (byte)(6 + request.SlotTo));
            _game.PacketNotifier.NotifyItemsSwapped(
                champion,
                request.SlotFrom,
                request.SlotTo
            );

            return true;
        }
    }
}
