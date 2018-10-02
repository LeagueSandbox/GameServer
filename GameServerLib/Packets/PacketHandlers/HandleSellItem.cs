using GameServerCore;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase
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

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadSellItemRequest(data);
            var champion = (Champion)_playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemSellRequest(request.SlotId);
        }
    }
}
