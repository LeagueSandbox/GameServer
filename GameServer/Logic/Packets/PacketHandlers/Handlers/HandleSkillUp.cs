using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSkillUp : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var skillUpPacket = new SkillUpPacket(data);
            //!TODO Check if can up skill? :)

            var s = _playerManager.GetPeerInfo(peer).GetChampion().levelUpSpell(skillUpPacket.skill);

            if (s == null)
                return false;

            var skillUpResponse = new SkillUpPacket(
                _playerManager.GetPeerInfo(peer).GetChampion().getNetId(),
                skillUpPacket.skill,
                (byte)s.getLevel(),
                (byte)s.getOwner().getSkillPoints()
            );
            _game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            _playerManager.GetPeerInfo(peer).GetChampion().GetStats().setSpellEnabled(skillUpPacket.skill, true);

            return true;
        }
    }
}
