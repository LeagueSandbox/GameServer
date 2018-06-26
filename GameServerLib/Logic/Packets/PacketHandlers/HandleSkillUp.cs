using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSkillUp : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SkillUp;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSkillUp(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var skillUpPacket = new SkillUpRequest(data);
            //!TODO Check if can up skill? :)

            var s = _playerManager.GetPeerInfo(peer).Champion.LevelUpSpell(skillUpPacket.skill);
            if (s == null)
                return false;

            var skillUpResponse = new SkillUpResponse(
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                skillUpPacket.skill,
                (byte)s.Level,
                (byte)s.Owner.getSkillPoints()
            );
            _game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            _playerManager.GetPeerInfo(peer).Champion.Stats.SetSpellEnabled(skillUpPacket.skill, true);

            return true;
        }
    }
}
