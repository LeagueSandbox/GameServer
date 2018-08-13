using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SWAP_ITEMS;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSwapItems(Game game)
        {
            _packetReader = game.PacketReader;
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadSwapItemsRequest(data);
            if (request.SlotFrom > 6 || request.SlotTo > 6)
            {
                return false;
            }

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            _playerManager.GetPeerInfo(peer).Champion.Inventory.SwapItems(request.SlotFrom, request.SlotTo);
            _game.PacketNotifier.NotifyItemsSwapped(
                _playerManager.GetPeerInfo(peer).Champion,
                request.SlotFrom,
                request.SlotTo
            );

            return true;
        }
    }
}
