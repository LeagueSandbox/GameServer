using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "skillpoints";
        public override string Syntax => "";

        public SkillpointsCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager) : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            _playerManager.GetPeerInfo(peer).Champion.setSkillPoints(17);
            var skillUpResponse = new SkillUpPacket(_playerManager.GetPeerInfo(peer).Champion.NetId, 0, 0, 17);
            _game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
        }
    }
}
