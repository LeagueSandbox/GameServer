using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public SkillpointsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            champion.SkillPoints = 17;

            _game.PacketNotifier.NotifySkillUp(userId, champion.NetId, 0, 0, 17);
        }
    }
}
