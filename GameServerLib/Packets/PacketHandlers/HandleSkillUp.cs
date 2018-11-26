using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSkillUp : PacketHandlerBase<SkillUpRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SKILL_UP;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSkillUp(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SkillUpRequest req)
        {
            //!TODO Check if can up skill? :)

            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var s = champion.LevelUpSpell(req.Skill);
            if (s == null)
            {
                return false;
            }

             _game.PacketNotifier.NotifySkillUp(userId, champion.NetId, req.Skill, s.Level, s.Owner.SkillPoints);
            champion.Stats.SetSpellEnabled(req.Skill, true);

            return true;
        }
    }
}
