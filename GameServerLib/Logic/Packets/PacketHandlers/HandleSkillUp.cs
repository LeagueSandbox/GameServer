using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSkillUp : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SKILL_UP;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleSkillUp(Game game)
        {
            _game = game;
            _playerManager = game.GetPlayerManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var skillUpPacket = new SkillUpRequest(data);
            //!TODO Check if can up skill? :)

            var s = _playerManager.GetPeerInfo(peer).Champion.LevelUpSpell(skillUpPacket.Skill);
            if (s == null)
            {
                return false;
            }

            var skillUpResponse = new SkillUpResponse(_game,
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                skillUpPacket.Skill,
                (byte)s.Level,
                (byte)s.Owner.GetSkillPoints()
            );
            _game.PacketHandlerManager.SendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            _playerManager.GetPeerInfo(peer).Champion.Stats.SetSpellEnabled(skillUpPacket.Skill, true);

            return true;
        }
    }
}
