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

            var s = game.getPeerInfo(peer).getChampion().levelUpSpell(skillUpPacket.skill);

            if (s == null)
                return false;

            var skillUpResponse = new SkillUpPacket(game.getPeerInfo(peer).getChampion().getNetId(), skillUpPacket.skill, (byte)s.getLevel(), (byte)s.getOwner().getSkillPoints());
            PacketHandlerManager.getInstace().sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            game.getPeerInfo(peer).getChampion().getStats().setSpellEnabled(skillUpPacket.skill, true);

            return true;
        }
    }
}
