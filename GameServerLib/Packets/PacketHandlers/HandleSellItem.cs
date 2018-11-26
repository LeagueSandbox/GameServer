using GameServerCore;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Items;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase<SellItemRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SELL_ITEM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSellItem(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SellItemRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemSellRequest(req.SlotId);
        }
    }
}
