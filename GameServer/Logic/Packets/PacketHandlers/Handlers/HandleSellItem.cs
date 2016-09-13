using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSellItem : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
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
                _game.PacketNotifier.notifyRemoveItem(client.Champion, sell.slotId, i.StackSize);
                if (i.StackSize == 0)
                {
                    client.Champion.getInventory().RemoveItem(sell.slotId);
                }
            }
            else
            {
                _game.PacketNotifier.notifyRemoveItem(client.Champion, sell.slotId, 0);
                client.Champion.getInventory().RemoveItem(sell.slotId);
            }

            client.Champion.GetStats().RemoveBuff(i.ItemType);

            return true;
        }
    }
}
