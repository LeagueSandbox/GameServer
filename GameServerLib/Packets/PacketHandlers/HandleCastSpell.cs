using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;

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
            var targetObj = _game.ObjectManager.GetObjectById(req.TargetNetID);
            var targetUnit = targetObj as IAttackableUnit;
            var owner = _playerManager.GetPeerInfo(userId).Champion;
            if (owner == null)
            {
                return false;
            }

            var s = owner.GetSpell(req.Slot);
            var ownerCastingSpell = owner.GetCastSpell();

            // Instant cast spells can be cast during other spell casts.
            if (s != null && owner.CanCast(s)
                && (ownerCastingSpell == null
                || (ownerCastingSpell != null
                    && s.SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    && !ownerCastingSpell.SpellData.CantCancelWhileWindingUp))
            {
                if (s.Cast(req.Position, req.EndPosition, targetUnit))
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
            }

            return false;
        }
    }
}
