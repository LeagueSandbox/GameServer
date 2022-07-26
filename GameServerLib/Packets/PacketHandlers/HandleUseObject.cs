using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase<UseObjectRequest>
    {
        private readonly Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly PlayerManager _playerManager;

        public HandleUseObject(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, UseObjectRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var target = _game.ObjectManager.GetObjectById(req.TargetNetID) as AttackableUnit;

            champion.SetSpell(target.CharData.HeroUseSpell, (byte)SpellSlotType.UseSpellSlot, true);

            var s = champion.Spells[(short)SpellSlotType.UseSpellSlot];
            var ownerCastingSpell = champion.GetCastSpell();

            // Instant cast spells can be cast during other spell casts.
            if (s != null && champion.CanCast(s)
                && champion.ChannelSpell == null
                && (ownerCastingSpell == null
                || (ownerCastingSpell != null
                    && s.SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    && !ownerCastingSpell.SpellData.CantCancelWhileWindingUp))
            {
                s.Cast(target.Position, target.Position, target);
                return true;
            }

            return false;
        }
    }
}
