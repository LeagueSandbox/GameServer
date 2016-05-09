using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSkillUp : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var skillUpPacket = new SkillUpPacket(data);
            //!TODO Check if can up skill? :)

            var s = game.GetPeerInfo(peer).GetChampion().levelUpSpell(skillUpPacket.skill);

            if (s == null)
                return false;

            var skillUpResponse = new SkillUpPacket(game.GetPeerInfo(peer).GetChampion().getNetId(), skillUpPacket.skill, (byte)s.getLevel(), (byte)s.getOwner().getSkillPoints());
            game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            game.GetPeerInfo(peer).GetChampion().getStats().setSpellEnabled(skillUpPacket.skill, true);

            return true;
        }
    }
}
