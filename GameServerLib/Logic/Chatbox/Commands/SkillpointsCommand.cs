using ENet;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public SkillpointsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            champion.SetSkillPoints(17);

            _game.PacketNotifier.NotifySkillUp(peer, champion.NetId, 0, 0, 17);
        }
    }
}
