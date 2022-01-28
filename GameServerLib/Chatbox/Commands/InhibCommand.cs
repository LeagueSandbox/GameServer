using GameServerCore;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class InhibCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "inhib";
        public override string Syntax => $"{Command}";

        public InhibCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var sender = _playerManager.GetPeerInfo(userId);
            var min = new Minion(
                Game,
                null,
                sender.Champion.Position,
                "Worm",
                "Worm",
                aiScript: "BasicJungleMonsterAi"
                );
            Game.ObjectManager.AddObject(min);
        }
    }
}
