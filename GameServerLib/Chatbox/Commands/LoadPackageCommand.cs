using System;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    class LoadPackageCommand : ChatCommandBase
    {
        public override string Command => "loadpackage";
        public override string Syntax => $"{Command} packagename";

        private ContentManager _contentManager;

        public LoadPackageCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager, game)
        {
            _contentManager = game.Config.ContentManager;
        }

        
        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length >= 2)
            {
                string packageName = split[1];

                _contentManager.LoadPackage(packageName);
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, $"Loading content from package: {packageName}");
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
