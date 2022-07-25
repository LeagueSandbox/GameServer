using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class TpCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "tp";
        public override string Syntax => $"{Command} [target NetID (0 or none for self)] X Y";

        public TpCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            uint targetNetID = 0;
            float x, y;

            if (split.Length < 3 || split.Length > 4)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }

            if (split.Length > 3 && uint.TryParse(split[1], out targetNetID) && float.TryParse(split[2], out x) && float.TryParse(split[3], out y))
            {
                var obj = Game.ObjectManager.GetObjectById(targetNetID);
                if (obj != null)
                {
                    obj.TeleportTo(x, y);
                }
                else
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, "An object with the netID: " + targetNetID + " was not found.");
                    ShowSyntax();
                }
            }
            else if (float.TryParse(split[1], out x) && float.TryParse(split[2], out y))
            {
                _playerManager.GetPeerInfo(userId).Champion.TeleportTo(x, y);
            }
        }
    }
}