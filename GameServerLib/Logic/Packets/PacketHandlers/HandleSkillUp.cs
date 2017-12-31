﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSkillUp : PacketHandlerBase<SkillUpRequest>
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

        public override bool HandlePacketInternal(Peer peer, SkillUpRequest data)
        {
            //!TODO Check if can up skill? :)

            var s = _playerManager.GetPeerInfo(peer).Champion.LevelUpSpell(data.Skill);
            if (s == null)
                return false;

            var skillUpResponse = new SkillUpResponse(
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                data.Skill,
                (byte)s.Level,
                (byte)s.Owner.getSkillPoints()
            );
            _game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            _playerManager.GetPeerInfo(peer).Champion.GetStats().setSpellEnabled(data.Skill, true);

            return true;
        }
    }
}
