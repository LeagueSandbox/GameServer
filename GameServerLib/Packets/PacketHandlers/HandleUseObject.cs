using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase<UseObjectRequest>
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public HandleUseObject(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, UseObjectRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var target = _game.ObjectManager.GetObjectById(req.TargetNetID) as IAttackableUnit;

            if (target is IObjAiBase obj)
            {
                champion.SetSpell(obj.CharData.HeroUseSpell, (byte)SpellSlotType.UseSpellSlot, true);
            }

            var s = champion.GetSpell((byte)SpellSlotType.UseSpellSlot);
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
