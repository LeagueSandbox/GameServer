using System;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    class PrintPackageListCommand : ChatCommandBase
    {
        public override string Command => "showpackages";
        public override string Syntax => $"{Command}";

        private ContentManager _contentManager;

        public PrintPackageListCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager, game)
        {
            _contentManager = game.Config.ContentManager;
        }

        
        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var packageList = _contentManager.GetAllLoadedPackages();

            var printedString = "Loaded packages:\n";

            foreach (var dataPackage in packageList)
            {
                printedString += $"- {dataPackage.PackageName}\n";
            }

            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, printedString);

        }
    }
}
