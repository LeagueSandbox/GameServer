using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUpgradeSpellReq : PacketHandlerBase<UpgradeSpellReq>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleUpgradeSpellReq(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, UpgradeSpellReq req)
        {
            // TODO: Check if can up skill
            // TODO: Implement usage of req.IsEvolve

            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var s = champion.LevelUpSpell(req.Slot);
            if (s == null)
            {
                return false;
            }

            _game.PacketNotifier.NotifyNPC_UpgradeSpellAns(userId, champion.NetId, req.Slot, s.CastInfo.SpellLevel, champion.SkillPoints);
            champion.Stats.SetSpellEnabled(req.Slot, true);

            return true;
        }
    }
}
