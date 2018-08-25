﻿using ENet;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class HelpCommand : ChatCommandBase
    {
        private const string COMMAND_PREFIX = "<font color =\"#E175FF\"><b>";
        private const string COMMAND_SUFFIX = "</b></font>, ";

        public override string Command => "help";
        public override string Syntax => $"{Command}";

        public HelpCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {

        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            if (!Game.Config.ChatCheatsEnabled)
            {
                var msg = "[LS] Chat commands are disabled in this game.";
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, msg);
                return;
            }

            var commands = ChatCommandManager.GetCommandsStrings();
            var commandsString = "";
            foreach (var command in commands)
            {
                commandsString = $"{commandsString}{COMMAND_PREFIX}" +
                                 $"{ChatCommandManager.CommandStarterCharacter}{command}" +
                                 $"{COMMAND_SUFFIX}";
            }

            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, commandsString);
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + commands.Count + " commands");
        }
    }
}
