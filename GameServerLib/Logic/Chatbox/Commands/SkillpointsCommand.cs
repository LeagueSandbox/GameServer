using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public SkillpointsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _playerManager.GetPeerInfo(peer).Champion.SetSkillPoints(17);
            var skillUpResponse = new SkillUpResponse(Game, _playerManager.GetPeerInfo(peer).Champion.NetId, 0, 0, 17);
            Game.PacketHandlerManager.SendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
        }
    }
}
