using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class LevelCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "level";
        public override string Syntax => $"{Command} level";
        private readonly Game _game;

        public LevelCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            var champ = _playerManager.GetPeerInfo(userId).Champion;
            var maxLevel = _game.Map.MapScript.MapScriptMetadata.MaxLevel;

            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (byte.TryParse(split[1], out var lvl))
            {
                if (lvl <= champ.Stats.Level || lvl > maxLevel)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, $"The level must be higher than current and smaller or equal to what the gamemode allows({maxLevel})!");
                    return;
                }

                while (champ.Stats.Level < lvl)
                {
                    champ.LevelUp(true);
                }
            }
        }
    }
}
