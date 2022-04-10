using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase<RemoveItemReq>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleSellItem(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, RemoveItemReq req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemSellRequest(req.Slot);
        }
    }
}
