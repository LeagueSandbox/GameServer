using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Content;
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
            var client = _playerManager.GetPeerInfo(userId);

            var i = _playerManager.GetPeerInfo(userId).Champion.Inventory.GetItem(request.SlotId) as Item;
            if (i == null)
            {
                return false;
            }

            var sellPrice = i.ItemType.TotalPrice * i.ItemType.SellBackModifier;
            client.Champion.Stats.Gold += sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, request.SlotId, i.StackSize);
                if (i.StackSize == 0)
                {
                    client.Champion.RemoveSpell((byte)(request.SlotId + Shop.ITEM_ACTIVE_OFFSET));
                    client.Champion.Inventory.RemoveItem(request.SlotId);
                }
            }
            else
            {
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, request.SlotId, 0);
                client.Champion.RemoveSpell((byte)(request.SlotId + Shop.ITEM_ACTIVE_OFFSET));
                client.Champion.Inventory.RemoveItem(request.SlotId);
            }

            client.Champion.Stats.RemoveModifier(i.ItemType);

            return true;
        }
    }
}
