using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            PlayerManager.GetPeerInfo(peer).Champion.SetSkillPoints(17);
            var skillUpResponse = new SkillUpResponse(PlayerManager.GetPeerInfo(peer).Champion.NetId, 0, 0, 17);
            Game.PacketHandlerManager.SendPacket(peer, skillUpResponse, Channel.CHLGamePLAY);
        }
    }
}
