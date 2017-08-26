using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase<SellItem>
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

        public override bool HandlePacketInternal(Peer peer, SellItem data)
        {
            var client = _playerManager.GetPeerInfo(peer);

            var i = _playerManager.GetPeerInfo(peer).Champion.getInventory().GetItem(data.SlotId);
            if (i == null)
                return false;

            float sellPrice = i.ItemType.TotalPrice * i.ItemType.SellBackModifier;
            client.Champion.GetStats().Gold += sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, data.SlotId, i.StackSize);
                if (i.StackSize == 0)
                {
                    client.Champion.getInventory().RemoveItem(data.SlotId);
                }
            }
            else
            {
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, data.SlotId, 0);
                client.Champion.getInventory().RemoveItem(data.SlotId);
            }

            client.Champion.GetStats().RemoveModifier(i.ItemType);

            return true;
        }
    }
}
