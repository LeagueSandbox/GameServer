﻿using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ModelCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "model";
        public override string Syntax => $"{Command} modelName";

        public ModelCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length >= 2)
            {
                _playerManager.GetPeerInfo(peer).Champion.Model = split[1];
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
