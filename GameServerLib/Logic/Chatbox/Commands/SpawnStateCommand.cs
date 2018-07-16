﻿using ENet;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SpawnStateCommand : ChatCommandBase
    {
        public override string Command => "spawnstate";
        public override string Syntax => $"{Command} 0 (disable) / 1 (enable)";

        public SpawnStateCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {

        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2 || !byte.TryParse(split[1], out var input) || input > 1)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else
            {
                Game.Map.MapGameScript.SpawnEnabled = input != 0;
            }
        }
    }
}
