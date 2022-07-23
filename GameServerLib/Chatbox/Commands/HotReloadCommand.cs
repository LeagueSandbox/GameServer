using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class HotReloadCommand : ChatCommandBase
    {
        public override string Command => "hotreload";
        public override string Syntax => $"{Command} 0 (disable) / 1 (enable)";

        public HotReloadCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2 || !byte.TryParse(split[1], out byte input) || input > 1)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else
            {
                if (input == 1)
                {
                    Game.EnableHotReload(true);
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts hot reload enabled.");
                }
                else
                {
                    Game.EnableHotReload(false);
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Scripts hot reload disabled.");
                }

            }
        }
    }
}
