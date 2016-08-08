using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SkillpointsCommand : ChatCommand
    {
        public SkillpointsCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _owner.GetGame().GetPeerInfo(peer).GetChampion().setSkillPoints(17);
            var skillUpResponse = new SkillUpPacket(_owner.GetGame().GetPeerInfo(peer).GetChampion().getNetId(), 0, 0, 17);
            _owner.GetGame().PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
        }
    }
}
