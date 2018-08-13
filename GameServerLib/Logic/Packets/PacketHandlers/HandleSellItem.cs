using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SELL_ITEM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSellItem(Game game)
        {
            _packetReader = game.PacketReader;
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadSellItemRequest(data);
            var client = _playerManager.GetPeerInfo(peer);

            var i = _playerManager.GetPeerInfo(peer).Champion.GetInventory().GetItem(request.SlotId);
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
                    client.Champion.GetInventory().RemoveItem(request.SlotId);
                }
            }
            else
            {
                _game.PacketNotifier.NotifyRemoveItem(client.Champion, request.SlotId, 0);
                client.Champion.GetInventory().RemoveItem(request.SlotId);
            }

            client.Champion.Stats.RemoveModifier(i.ItemType);

            return true;
        }
    }
}
