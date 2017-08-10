using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleSellItem : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SellItem;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSellItem(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var sell = new SellItem(data);
            var client = _playerManager.GetPeerInfo(peer);

            var i = _playerManager.GetPeerInfo(peer).Champion.getInventory().GetItem(sell.slotId);
            if (i == null)
                return false;

            float sellPrice = i.ItemType.TotalPrice * i.ItemType.SellBackModifier;
            client.Champion.GetStats().Gold += sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, sell.slotId, i.StackSize);
                if (i.StackSize == 0)
                {
                    client.Champion.getInventory().RemoveItem(sell.slotId);
                }
            }
            else
            {
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, sell.slotId, 0);
                client.Champion.getInventory().RemoveItem(sell.slotId);
            }

            client.Champion.GetStats().RemoveModifier(i.ItemType);

            return true;
        }
    }
}
