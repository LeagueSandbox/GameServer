﻿using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSkillUp : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SKILL_UP;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSkillUp(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadSkillUpRequest(data);
            //!TODO Check if can up skill? :)

            var champion = _playerManager.GetPeerInfo(peer).Champion;
            var s = champion.LevelUpSpell(request.Skill);
            if (s == null)
            {
                return false;
            }

             _game.PacketNotifier.NotifySkillUp(peer, champion.NetId, request.Skill, (byte)s.Level, (byte)s.Owner.GetSkillPoints());
            champion.Stats.SetSpellEnabled(request.Skill, true);

            return true;
        }
    }
}
