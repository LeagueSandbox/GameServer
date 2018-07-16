﻿using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class XpCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "xp";
        public override string Syntax => $"{Command} xp";

        public XpCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }

            if (float.TryParse(split[1], out var xp))
            {
                _playerManager.GetPeerInfo(peer).Champion.Stats.Experience += xp;
            }
        }
    }
}
