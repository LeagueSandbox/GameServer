using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase<CastSpellRequest>
    {
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly IPlayerManager _playerManager;

        public HandleCastSpell(Game game)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, CastSpellRequest req)
        {
            var targetObj = _game.ObjectManager.GetObjectById(req.TargetNetId);
            var targetUnit = targetObj as IAttackableUnit;
            var owner = _playerManager.GetPeerInfo(userId).Champion;
            if (owner == null || !owner.CanCast())
            {
                return false;
            }

            var s = owner.GetSpell(req.SpellSlot);
            if (s == null)
            {
                return false;
            }

            if(s.Cast(req.Position, req.EndPosition, targetUnit))
            {
                if (s.CastInfo.SpellSlot >= (int)SpellSlotType.InventorySlots && s.CastInfo.SpellSlot < (int)SpellSlotType.BluePillSlot)
                {
                    var item = s.CastInfo.Owner.Inventory.GetItem(s.SpellName);
                    if (item != null && item.ItemData.Consumed)
                    {
                        var inventory = owner.Inventory;
                        inventory.RemoveItem(inventory.GetItemSlot(item), owner);
                    }
                }
                return true;
            }

            return false;
        }
    }
}
